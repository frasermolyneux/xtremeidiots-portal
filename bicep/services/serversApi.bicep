targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string

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

param parTags object

// Variables
var varServersWebAppName = 'webapi-servers-portal-${parEnvironment}-${parLocation}'
var varWorkloadName = 'webapi-servers-portal-${parEnvironment}'

// Module Resources
module adminWebAppGeoLocationApiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'adminWebAppGeoLocationApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varServersWebAppName
    parKeyVaultName: parKeyVaultName
    parSubscriptionScopeIdentifier: 'portal-repository'
    parSubscriptionScope: '/apis/repository-api'
    parTags: parTags
  }
}

module webApp 'serversApi/webApp.bicep' = {
  name: 'serversApiWebApp'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parWebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
    parServersApiAppId: parServersApiAppId
    parStrategicServicesSubscriptionId: parStrategicServicesSubscriptionId
    parApiManagementResourceGroupName: parApiManagementResourceGroupName
    parApiManagementName: parApiManagementName
    parAppServicePlanName: parAppServicePlanName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parTags: parTags
  }
}

module webAppKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'serversApiKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module webAppStagingKeyVaultAccessPolicy './../modules/keyVaultAccessPolicy.bicep' = {
  name: 'serversApiStagingKeyVaultAccessPolicy'

  params: {
    parKeyVaultName: parKeyVaultName
    parPrincipalId: webApp.outputs.outWebAppStagingIdentityPrincipalId
  }
}

module apiManagementServersApi 'serversApi/apiManagementApi.bicep' = {
  name: 'apiManagementServersApi'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: parApiManagementName
    parFrontDoorDns: varWorkloadName
    parParentDnsName: parParentDnsName
    parEnvironment: parEnvironment
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parKeyVaultName: parKeyVaultName
    parAppInsightsName: parAppInsightsName
  }
}

module frontDoorEndpoint './../modules/frontDoorEndpoint.bicep' = {
  name: 'serversApiFrontDoorEndpoint'
  scope: resourceGroup(parConnectivitySubscriptionId, parFrontDoorResourceGroupName)

  params: {
    parFrontDoorName: parFrontDoorName
    parParentDnsName: parParentDnsName
    parDnsResourceGroupName: parDnsResourceGroupName
    parWorkloadName: varWorkloadName
    parOriginHostName: webApp.outputs.outWebAppDefaultHostName
    parDnsZoneHostnamePrefix: varWorkloadName
    parCustomHostname: '${varWorkloadName}.${parParentDnsName}'
    parTags: parTags
  }
}
