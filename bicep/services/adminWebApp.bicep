targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string

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
var varDeploymentPrefix = 'adminWebApp' //Prevent deployment naming conflicts
var varAdminWebAppName = 'webapp-admin-portal-${parEnvironment}-${parLocation}'
var varWorkloadName = 'webapp-admin-portal-${parEnvironment}'

// Module Resources
module geolocationApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-geolocationApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'geolocation'
    parSubscriptionScope: '/apis/geolocation-api'
    parTags: parTags
  }
}

module repositoryApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-repositoryApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'portal-repository'
    parSubscriptionScope: '/apis/repository-api'
    parTags: parTags
  }
}

module serversApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-serversApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'portal-servers'
    parSubscriptionScope: '/apis/servers-api'
    parTags: parTags
  }
}

module webApp 'adminWebApp/webApp.bicep' = {
  name: '${varDeploymentPrefix}-webApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName

    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parSqlServerResourceGroupName: parSqlServerResourceGroupName
    parSqlServerName: parSqlServerName

    parConnectivitySubscriptionId: parConnectivitySubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parFrontDoorName: parFrontDoorName

    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name

    parTags: parTags
  }
}

module keyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-keyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module slotKeyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:V2022.07.31.6322' = if (parEnvironment == 'prd') {
  name: '${varDeploymentPrefix}-slotKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppStagingIdentityPrincipalId
  }
}

module sqlDatabase 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/sqldatabase:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-sqlDatabase'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parSqlServerResourceGroupName)

  params: {
    parSqlServerName: parSqlServerName
    parLocation: parLocation
    parDatabaseName: 'portalidentitydb-${parEnvironment}'
    parSkuCapacity: 5
    parSkuName: 'Basic'
    parSkuTier: 'Basic'
    parTags: parTags
  }
}

module frontDoorEndpoint 'adminWebApp/frontDoorEndpoint.bicep' = {
  name: '${varDeploymentPrefix}-frontDoorEndpoint'
  scope: resourceGroup(parConnectivitySubscriptionId, parFrontDoorResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
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
