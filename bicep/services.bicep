targetScope = 'resourceGroup'

param parLocation string
param parEnvironment string

param parEventsApiAppId string
param parRepositoryApiAppId string
param parServersApiAppId string

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
var varKeyVaultName = 'kv-portal-${parEnvironment}-${parLocation}'
var varAppInsightsName = 'ai-portal-${parEnvironment}-${parLocation}'

var varServiceBusName = 'sb-portal-${parEnvironment}-${parLocation}'

module repositoryApi 'services/repositoryApi.bicep' = {
  name: 'repositoryApi'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parRepositoryApiAppId: parRepositoryApiAppId
    parConnectivitySubscriptionId: parConnectivitySubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parDnsResourceGroupName: parDnsResourceGroupName
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parSqlServerResourceGroupName: parSqlServerResourceGroupName
    parSqlServerName: parSqlServerName
    parTags: parTags
  }
}

module serversApi 'services/serversApi.bicep' = {
  name: 'serversApi'

  dependsOn: [
    repositoryApi
  ]

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parServersApiAppId: parServersApiAppId
    parConnectivitySubscriptionId: parConnectivitySubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parDnsResourceGroupName: parDnsResourceGroupName
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}

module adminWebApp 'services/adminWebApp.bicep' = {
  name: 'adminWebApp'

  dependsOn: [
    repositoryApi
    serversApi
  ]

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parConnectivitySubscriptionId: parConnectivitySubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parDnsResourceGroupName: parDnsResourceGroupName
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parSqlServerResourceGroupName: parSqlServerResourceGroupName
    parSqlServerName: parSqlServerName
    parTags: parTags
  }
}

module eventsApp 'services/eventsApp.bicep' = {
  name: 'eventsApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parServiceBusName: varServiceBusName
    parEventsApiAppId: parEventsApiAppId
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}

//module ingestApp 'services/ingestApp.bicep' = {
//  name: 'ingestApp'
//  params: {
//    parLocation: parLocation
//    parEnvironment: parEnvironment
//    parKeyVaultName: varKeyVaultName
//    parAppServicePlanName: varAppServicePlanName
//    parAppInsightsName: varAppInsightsName
//    parApiManagementName: varApimName
//    parServiceBusName: varServiceBusName
//  }
//}
//

//
//module repositoryApp 'services/repositoryApp.bicep' = {
//  name: 'repositoryApp'
//  params: {
//    parLocation: parLocation
//    parEnvironment: parEnvironment
//    parKeyVaultName: varKeyVaultName
//    parAppServicePlanName: varAppServicePlanName
//    parAppInsightsName: varAppInsightsName
//    parApiManagementName: varApimName
//    parServiceBusName: varServiceBusName
//    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
//    parApiManagementResourceGroupName: parApiManagementResourceGroupName
//    parPlatformApiManagementName: parApiManagementName
//    parTags: parTags
//  }
//}
//

//
//module syncApp 'services/syncApp.bicep' = {
//  name: 'syncApp'
//  params: {
//    parLocation: parLocation
//    parEnvironment: parEnvironment
//    parKeyVaultName: varKeyVaultName
//    parAppServicePlanName: varAppServicePlanName
//    parAppInsightsName: varAppInsightsName
//    parApiManagementName: varApimName
//    parServiceBusName: varServiceBusName
//  }
//}
