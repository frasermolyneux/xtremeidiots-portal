targetScope = 'subscription'

param parLocation string
param parEnvironment string

param parSqlAdminGroupOid string
@secure()
param parSqlAdminPassword string

param parLoggingSubscriptionId string
param parLoggingResourceGroupName string
param parLoggingWorkspaceName string
param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parTags object

// Variables
//var varResourceGroupName = 'rg-portal-${parEnvironment}-${parLocation}-01'
//var varKeyVaultName = 'kv-portal-${parEnvironment}-${parLocation}-01'
//var varLogWorkspaceName = 'log-portal-${parEnvironment}-${parLocation}-01'
//var varAppInsightsName = 'ai-portal-${parEnvironment}-${parLocation}-01'
//var varApimName = 'apim-portal-${parEnvironment}-${parLocation}-01'
//var varAppServicePlanName = 'plan-portal-${parEnvironment}-${parLocation}-01'
//var varServiceBusName = 'sb-portal-${parEnvironment}-${parLocation}-01'

//resource portalResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
//  name: varResourceGroupName
//  location: parLocation
//  properties: {}
//}
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
