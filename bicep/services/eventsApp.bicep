targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parServiceBusName string

param parEventsApiAppId string

param parStrategicServicesSubscriptionId string
param parWebAppsResourceGroupName string
param parAppServicePlanName string

param parTags object

// Module Resources
module storageAccount './../modules/funcAppStorageAccount.bicep' = {
  name: 'eventsFuncStorageAccount'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parWorkloadName: 'eventsfn'
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module functionApp 'eventsApp/functionApp.bicep' = {
  name: 'eventsAppFunctionApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parServiceBusName: parServiceBusName
    parStorageAccountName: storageAccount.outputs.outStorageAccountName
    parEventsApiAppId: parEventsApiAppId
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module functionAppKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'eventsAppFunctionAppKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppIdentityPrincipalId
  }
}

module functionAppStagingKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'eventsAppFunctionAppStagingKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppStagingIdentityPrincipalId
  }
}

module serviceBusQueues 'eventsApp/serviceBusQueues.bicep' = {
  name: 'serviceBusQueues'

  params: {
    parServiceBusName: parServiceBusName
  }
}
