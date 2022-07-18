targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parServiceBusName string

param parEventsApiAppId string

param parParentDnsName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string

param parTags object

// Variables
var varDeploymentPrefix = 'eventsApp' //Prevent deployment naming conflicts
var varWorkloadName = 'fn-events-portal-${parEnvironment}'

// Module Resources
module funcAppStorageAccount './../modules/funcAppStorageAccount.bicep' = {
  name: '${varDeploymentPrefix}-funcAppStorageAccount'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parWorkloadName: 'eventsfn'
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module functionApp 'eventsApp/functionApp.bicep' = {
  name: '${varDeploymentPrefix}-functionApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parServiceBusName: parServiceBusName
    parStorageAccountName: funcAppStorageAccount.outputs.outStorageAccountName
    parEventsApiAppId: parEventsApiAppId
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module keyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: '${varDeploymentPrefix}-keyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppIdentityPrincipalId
  }
}

module slotKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: '${varDeploymentPrefix}-slotKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppStagingIdentityPrincipalId
  }
}

module serviceBusQueues 'eventsApp/serviceBusQueues.bicep' = {
  name: '${varDeploymentPrefix}-serviceBusQueues'

  params: {
    parServiceBusName: parServiceBusName
  }
}

module apiManagementApi 'eventsApp/apiManagementApi.bicep' = {
  name: '${varDeploymentPrefix}-apiManagementApi'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parFunctionAppName: functionApp.outputs.outFunctionAppName
    parFunctionAppHostname: functionApp.outputs.outFunctionAppDefaultHostName
    parEnvironment: parEnvironment
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
  }
}
