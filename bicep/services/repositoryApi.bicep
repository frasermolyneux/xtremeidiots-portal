targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string

param parRepositoryApiAppId string

param parFrontDoorSubscriptionId string
param parFrontDoorResourceGroupName string
param parFrontDoorName string

param parDnsSubscriptionId string
param parDnsResourceGroupName string
param parParentDnsName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string
param parSqlServerResourceGroupName string
param parSqlServerName string

param parTags object

// Variables
var varDeploymentPrefix = 'repositoryApi' //Prevent deployment naming conflicts
var varWorkloadName = 'webapi-repository-portal-${parEnvironment}'

// Module Resources
module appDataStorage 'repositoryApi/appDataStorage.bicep' = {
  name: '${varDeploymentPrefix}-appDataStorage'

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module webApp 'repositoryApi/webApp.bicep' = {
  name: '${varDeploymentPrefix}-webApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName

    parRepositoryApiAppId: parRepositoryApiAppId
    parAppDataStorageAccountName: appDataStorage.outputs.outStorageAccountName

    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parAppServicePlanName: parAppServicePlanName
    parSqlServerResourceGroupName: parSqlServerResourceGroupName
    parSqlServerName: parSqlServerName

    parFrontDoorSubscriptionId: parFrontDoorSubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parFrontDoorName: parFrontDoorName

    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name

    parTags: parTags
  }
}

module keyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:latest' = {
  name: '${varDeploymentPrefix}-keyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module slotKeyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:latest' = if (parEnvironment == 'prd') {
  name: '${varDeploymentPrefix}-slotKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppStagingIdentityPrincipalId
  }
}

module sqlDatabase 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/sqldatabase:latest' = {
  name: '${varDeploymentPrefix}-sqlDatabase'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parSqlServerResourceGroupName)

  params: {
    parSqlServerName: parSqlServerName
    parLocation: parLocation
    parDatabaseName: 'portaldb-${parEnvironment}'
    parSkuCapacity: 10
    parSkuName: 'Standard'
    parSkuTier: 'Standard'
    parTags: parTags
  }
}

module apiManagementApi 'repositoryApi/apiManagementApi.bicep' = {
  name: '${varDeploymentPrefix}-apiManagementApi'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parFrontDoorDns: varWorkloadName
    parParentDnsName: parParentDnsName
    parEnvironment: parEnvironment
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parAppInsightsName: parAppInsightsName
  }
}

module frontDoorEndpoint 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/frontdoorendpoint:latest' = {
  name: '${varDeploymentPrefix}-frontDoorEndpoint'
  scope: resourceGroup(parFrontDoorSubscriptionId, parFrontDoorResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parDnsSubscriptionId: parDnsSubscriptionId
    parDnsResourceGroupName: parDnsResourceGroupName
    parWorkloadName: varWorkloadName
    parOriginHostName: webApp.outputs.outWebAppDefaultHostName
    parDnsZoneHostnamePrefix: varWorkloadName
    parCustomHostname: '${varWorkloadName}.${parParentDnsName}'
    parTags: parTags
  }
}
