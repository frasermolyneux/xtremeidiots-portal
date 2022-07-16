targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parServiceBusName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string

param parTags object

// Variables
var varRepositoryFuncAppName = 'fn-repository-portal-${parEnvironment}-${parLocation}'

// Module Resources
module repositoryAppGeoLocationApiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'repositoryAppGeoLocationApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varRepositoryFuncAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'geolocation'
    parSubscriptionScope: '/apis/geolocation-api'
    parTags: parTags
  }
}

module repositoryAppRepositoryApiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'repositoryAppRepositoryApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varRepositoryFuncAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'portal-repository'
    parSubscriptionScope: '/apis/repository-api'
    parTags: parTags
  }
}

module repositoryAppServersApiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'repositoryAppServersApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varRepositoryFuncAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'portal-servers'
    parSubscriptionScope: '/apis/servers-api'
    parTags: parTags
  }
}

module storageAccount './../modules/funcAppStorageAccount.bicep' = {
  name: 'repositoryAppStorageAccount'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parWorkloadName: 'repofn'
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module functionApp 'repositoryApp/functionApp.bicep' = {
  name: 'repositoryAppFunctionApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parServiceBusName: parServiceBusName
    parStorageAccountName: storageAccount.outputs.outStorageAccountName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module functionAppKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'repositoryAppFunctionAppKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppIdentityPrincipalId
  }
}
