targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parServiceBusName string

param parConnectivitySubscriptionId string
param parFrontDoorResourceGroupName string
param parDnsResourceGroupName string
param parFrontDoorName string
param parAdminWebAppDnsPrefix string
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
var varAdminWebAppName = 'webapp-admin-portal-${parEnvironment}-${parLocation}'

// Module Resources
module adminWebAppGeoLocationApiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'adminWebAppGeoLocationApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'geoloc'
    parSubscriptionScope: '/apis/geolocation-api'
    parTags: parTags
  }
}

// TODO: Need subscriptions for repository and servers APIs

module webApp 'adminWebApp/webApp.bicep' = {
  name: 'adminWebApp'
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
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module webAppKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'adminWebAppKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module webAppStagingKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'adminWebAppStagingKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppStagingIdentityPrincipalId
  }
}

module webAppIdentitySqlDatabase './../modules/sqlDatabase.bicep' = {
  name: 'webAppIdentitySqlDatabase'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parSqlServerResourceGroupName)

  params: {
    parSqlServerName: parSqlServerName
    parLocation: parLocation
    parDatabaseName: 'portalidentity-${parEnvironment}'
    parSkuCapacity: 5
    parSkuName: 'Basic'
    parSkuTier: 'Basic'
    parTags: parTags
  }
}
