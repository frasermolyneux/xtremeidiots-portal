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
var varDeploymentPrefix = 'repositoryApp' //Prevent deployment naming conflicts
var varRepositoryFuncAppName = 'fn-repository-portal-${parEnvironment}-${parLocation}'

// Module Resources
module geolocationApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-geolocationApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
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

module repositoryApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-repositoryApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
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

module serversApiManagementSubscription 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/apimanagementsubscription:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-serversApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
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

module funcAppStorageAccount 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/funcappstorageaccount:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-funcAppStorageAccount'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parWorkloadName: 'repofn'
    parKeyVaultName: parKeyVaultName
    parTags: parTags
  }
}

module functionApp 'repositoryApp/functionApp.bicep' = {
  name: '${varDeploymentPrefix}-functionApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
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

module keyVaultAccessPolicy 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultaccesspolicy:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-keyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: functionApp.outputs.outFunctionAppIdentityPrincipalId
  }
}
