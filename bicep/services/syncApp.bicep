targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parWebAppsResourceGroupName string
param parAppServicePlanName string

param parTags object

// Variables
var varDeploymentPrefix = 'syncApp' //Prevent deployment naming conflicts
var varSyncFuncAppName = 'fn-sync-portal-${parEnvironment}-${parLocation}'

// Module Resources
module repositoryApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${varDeploymentPrefix}-repositoryApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
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
  name: '${varDeploymentPrefix}-appDataStorage'

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module funcAppStorageAccount 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/funcappstorageaccount:latest' = {
  name: '${varDeploymentPrefix}-funcAppStorageAccount'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parWorkloadName: 'syncfn'
    parTags: parTags
  }
}

module functionApp 'syncApp/functionApp.bicep' = {
  name: '${varDeploymentPrefix}-functionApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parStorageAccountName: funcAppStorageAccount.outputs.outStorageAccountName
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

module keyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:latest' = {
  name: '${varDeploymentPrefix}-keyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppIdentityPrincipalId
  }
}
