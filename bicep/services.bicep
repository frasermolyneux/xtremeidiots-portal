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
param parTags object

// Variables
var varKeyVaultName = 'kv-portal-${parEnvironment}-${parLocation}-01'
var varAppInsightsName = 'ai-portal-${parEnvironment}-${parLocation}-01'
var varApimName = 'apim-portal-${parEnvironment}-${parLocation}-01'
var varAppServicePlanName = 'plan-portal-${parEnvironment}-${parLocation}-01'
var varServiceBusName = 'sb-portal-${parEnvironment}-${parLocation}-01'
var varSqlServerName = 'sql-portal-${parEnvironment}-${parLocation}-01'

module adminWebApp 'services/adminWebApp.bicep' = {
  name: 'adminWebApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parSqlServerName: varSqlServerName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parPlatformApiManagementName: parApiManagementName
    parTags: parTags
  }
}

module eventsApp 'services/eventsApp.bicep' = {
  name: 'eventsApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parServiceBusName: varServiceBusName
    parEventsApiAppId: parEventsApiAppId
  }
}

module ingestApp 'services/ingestApp.bicep' = {
  name: 'ingestApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parServiceBusName: varServiceBusName
  }
}

module repositoryApi 'services/repositoryApi.bicep' = {
  name: 'repositoryApi'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parSqlServerName: varSqlServerName
    parRepositoryApiAppId: parRepositoryApiAppId
  }
}

module repositoryApp 'services/repositoryApp.bicep' = {
  name: 'repositoryApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parServiceBusName: varServiceBusName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parPlatformApiManagementName: parApiManagementName
    parTags: parTags
  }
}

module serversApi 'services/serversApi.bicep' = {
  name: 'serversApi'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parServersApiAppId: parServersApiAppId
  }
}

module syncApp 'services/syncApp.bicep' = {
  name: 'syncApp'
  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppServicePlanName: varAppServicePlanName
    parAppInsightsName: varAppInsightsName
    parApiManagementName: varApimName
    parServiceBusName: varServiceBusName
  }
}
