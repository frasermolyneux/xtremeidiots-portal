targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string

param parRepositoryApiAppId string

param parConnectivitySubscriptionId string
param parFrontDoorResourceGroupName string
param parDnsResourceGroupName string
param parFrontDoorName string
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
var varWorkloadName = 'webapi-repository-portal-${parEnvironment}'

// Module Resources
module appDataStorage 'repositoryApi/appDataStorage.bicep' = {
  name: 'repositoryApiAppDataStorage'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module webApp 'repositoryApi/webApp.bicep' = {
  name: 'repositoryApiWebApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parRepositoryApiAppId: parRepositoryApiAppId
    parAppDataStorageAccountName: appDataStorage.outputs.outStorageAccountName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parSqlServerResourceGroupName: parSqlServerResourceGroupName
    parSqlServerName: parSqlServerName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module webAppKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'repositoryApiWebAppKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module webAppStagingKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'repositoryApiWebAppStagingKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppStagingIdentityPrincipalId
  }
}

module apiManagementRepositoryApi 'repositoryApi/apiManagementApi.bicep' = {
  name: 'apiManagementRepositoryApi'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parFrontDoorDns: varWorkloadName
    parParentDnsName: parParentDnsName
    parEnvironment: parEnvironment
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
  }
}

module frontDoorEndpoint './../modules/frontDoorEndpoint.bicep' = {
  name: 'repositoryApiFrontDoorEndpoint'
  scope: resourceGroup(parConnectivitySubscriptionId, parFrontDoorResourceGroupName)

  params: {
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parDnsResourceGroupName: parDnsResourceGroupName
    parWorkloadName: varWorkloadName
    parOriginHostName: webApp.outputs.outWebAppDefaultHostName
    parDnsZoneHostnamePrefix: varWorkloadName
    parCustomHostname: '${varWorkloadName}.${parParentDnsName}'
    parTags: parTags
  }
}
