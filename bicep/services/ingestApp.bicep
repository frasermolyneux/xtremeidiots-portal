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
var varDeploymentPrefix = 'ingestApp' //Prevent deployment naming conflicts
var varIngestFuncAppName = 'fn-ingest-portal-${parEnvironment}-${parLocation}'

// Module Resources
module repositoryApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${varDeploymentPrefix}-repositoryApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varIngestFuncAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'portal-repository'
    parSubscriptionScope: '/apis/repository-api'
    parTags: parTags
  }
}

module funcAppStorageAccount 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/funcappstorageaccount:latest' = {
  name: '${varDeploymentPrefix}-funcAppStorageAccount'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parWorkloadName: 'ingestfn'
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module functionApp 'ingestApp/functionApp.bicep' = {
  name: '${varDeploymentPrefix}-functionApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parServiceBusName: parServiceBusName
    parStorageAccountName: funcAppStorageAccount.outputs.outStorageAccountName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module keyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:latest' = {
  name: '${varDeploymentPrefix}-keyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppIdentityPrincipalId
  }
}
