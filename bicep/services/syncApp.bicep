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
var varSyncFuncAppName = 'fn-sync-portal-${parEnvironment}-${parLocation}'

// Module Resources
module syncAppRepositoryApiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'syncAppRepositoryApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varSyncFuncAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'portal-repository'
    parSubscriptionScope: '/apis/repository-api'
    parTags: parTags
  }
}

module appDataStorage 'syncApp/appDataStorage.bicep' = {
  name: 'syncAppAppDataStorage'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module storageAccount './../modules/funcAppStorageAccount.bicep' = {
  name: 'syncAppStorageAccount'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parWorkloadName: 'syncfn'
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module functionApp 'syncApp/functionApp.bicep' = {
  name: 'syncAppFunctionApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parServiceBusName: parServiceBusName
    parStorageAccountName: storageAccount.outputs.outStorageAccountName
    parAppDataStorageAccountName: appDataStorage.outputs.outStorageAccountName
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
  name: 'syncAppFunctionAppKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppIdentityPrincipalId
  }
}
