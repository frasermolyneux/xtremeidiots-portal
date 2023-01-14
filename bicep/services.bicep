targetScope = 'resourceGroup'

param parLocation string
param parEnvironment string

param parEventsApiAppId string

param parFrontDoorSubscriptionId string
param parFrontDoorResourceGroupName string
param parFrontDoorName string

param parDnsSubscriptionId string
param parDnsResourceGroupName string
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

module adminWebApp 'services/adminWebApp.bicep' = {
  name: '${varDeploymentPrefix}-adminWebApp'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parFrontDoorSubscriptionId: parFrontDoorSubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName
    parDnsSubscriptionId: parDnsSubscriptionId
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

    parFrontDoorSubscriptionId: parFrontDoorSubscriptionId
    parFrontDoorResourceGroupName: parFrontDoorResourceGroupName

    parDnsSubscriptionId: parDnsSubscriptionId
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

module syncApp 'services/syncApp.bicep' = {
  name: '${varDeploymentPrefix}-syncApp'

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parWebAppsResourceGroupName: parWebAppsResourceGroupName
    parAppServicePlanName: parAppServicePlanName
    parTags: parTags
  }
}
