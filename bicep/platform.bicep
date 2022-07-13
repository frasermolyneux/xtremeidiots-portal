targetScope = 'subscription'

param parLocation string
param parEnvironment string

param parLoggingSubscriptionId string
param parLoggingResourceGroupName string
param parLoggingWorkspaceName string
param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parTags object

// Variables
var varResourceGroupName = 'rg-portal-${parEnvironment}-${parLocation}'
//var varKeyVaultName = 'kv-portal-${parEnvironment}-${parLocation}'
//var varLogWorkspaceName = 'log-portal-${parEnvironment}-${parLocation}'
//var varAppInsightsName = 'ai-portal-${parEnvironment}-${parLocation}'
//var varApimName = 'apim-portal-${parEnvironment}-${parLocation}'
//var varAppServicePlanName = 'plan-portal-${parEnvironment}-${parLocation}'
//var varServiceBusName = 'sb-portal-${parEnvironment}-${parLocation}'

resource portalResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  tags: parTags

  properties: {}
}
//
//// Platform
//module keyVault 'platform/keyVault.bicep' = {
//  name: 'keyVault'
//  scope: resourceGroup(portalResourceGroup.name)
//  params: {
//    parKeyVaultName: varKeyVaultName
//    parLocation: parLocation
//  }
//}
//
//module sqlServer 'platform/sqlServer.bicep' = {
//  name: 'sqlServer'
//  scope: resourceGroup(portalResourceGroup.name)
//  params: {
//    parLocation: parLocation
//    parEnvironment: parEnvironment
//    parAdminPassword: parSqlAdminPassword
//    parKeyVaultName: keyVault.outputs.outKeyVaultName
//    parAdminGroupOid: parSqlAdminGroupOid
//  }
//}
//
//module logging 'platform/logging.bicep' = {
//  name: 'logging'
//  scope: resourceGroup(portalResourceGroup.name)
//  params: {
//    parLogWorkspaceName: varLogWorkspaceName
//    parAppInsightsName: varAppInsightsName
//    parKeyVaultName: keyVault.outputs.outKeyVaultName
//    parLocation: parLocation
//  }
//}
//
//module apiManagment 'platform/apiManagement.bicep' = {
//  name: 'apiManagement'
//  scope: resourceGroup(portalResourceGroup.name)
//  params: {
//    parApimName: varApimName
//    parAppInsightsName: logging.outputs.outAppInsightsName
//    parKeyVaultName: keyVault.outputs.outKeyVaultName
//    parLocation: parLocation
//  }
//}
//
//module appServicePlan 'platform/appServicePlan.bicep' = {
//  name: 'webAppServicePlan'
//  scope: resourceGroup(portalResourceGroup.name)
//  params: {
//    parAppServicePlanName: varAppServicePlanName
//    parLocation: parLocation
//  }
//}
//
//module serviceBus 'platform/serviceBus.bicep' = {
//  name: 'serviceBus'
//  scope: resourceGroup(portalResourceGroup.name)
//  params: {
//    parServiceBusName: varServiceBusName
//    parKeyVaultName: keyVault.outputs.outKeyVaultName
//    parLocation: parLocation
//  }
//}
