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

var varDeploymentPrefix = 'portalServices' //Prevent deployment naming conflicts

module repositoryApi 'services/repositoryApi.bicep' = {
  name: '${varDeploymentPrefix}-repositoryApi'

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
  name: '${varDeploymentPrefix}-serversApi'

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
  name: '${varDeploymentPrefix}-adminWebApp'

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
  name: '${varDeploymentPrefix}-eventsApp'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parServiceBusName: varServiceBusName

    parEventsApiAppId: parEventsApiAppId

    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName

    parTags: parTags
  }
}

module ingestApp 'services/ingestApp.bicep' = {
  name: '${varDeploymentPrefix}-ingestApp'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parServiceBusName: varServiceBusName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}

module repositoryApp 'services/repositoryApp.bicep' = {
  name: '${varDeploymentPrefix}-repositoryApp'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parServiceBusName: varServiceBusName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}

module syncApp 'services/syncApp.bicep' = {
  name: '${varDeploymentPrefix}-syncApp'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parServiceBusName: varServiceBusName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}
