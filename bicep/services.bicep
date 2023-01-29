targetScope = 'resourceGroup'

param parLocation string
param parEnvironment string
param parInstance string

param parFrontDoor object
param parDns object
param parStrategicServices object

param parRepositoryApi object
param parServerIntegrationApi object
param parGeoLocationApi object

param parTags object

// Variables
var varEnvironmentUniqueId = uniqueString('portal-web', parEnvironment, parInstance)
var varDeploymentPrefix = 'services-${varEnvironmentUniqueId}' //Prevent deployment naming conflicts

var varKeyVaultName = 'kv-${varEnvironmentUniqueId}-${parLocation}'
var varAppInsightsName = 'ai-portal-web-${parEnvironment}-${parLocation}-${parInstance}'
var varWorkloadName = 'portal-web-${parEnvironment}-${parInstance}'
var varAdminWebAppName = 'app-portal-web-${parEnvironment}-${parLocation}-${parInstance}-${varEnvironmentUniqueId}'

// Module Resources
module geolocationApiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${varDeploymentPrefix}-geolocationApiManagementSubscription'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.ApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parApiManagementName: parStrategicServices.ApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: varKeyVaultName
    parSubscriptionScopeIdentifier: 'geolocation'
    parSubscriptionScope: '/apis/${parGeoLocationApi.ApimApiName}'
    parTags: parTags
  }
}

module repositoryApiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${varDeploymentPrefix}-repositoryApiManagementSubscription'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.ApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parApiManagementName: parStrategicServices.ApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: varKeyVaultName
    parSubscriptionScopeIdentifier: 'repository'
    parSubscriptionScope: '/apis/${parRepositoryApi.ApimApiName}'
    parTags: parTags
  }
}

module serversApiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${varDeploymentPrefix}-serversApiManagementSubscription'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.ApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parApiManagementName: parStrategicServices.ApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: varKeyVaultName
    parSubscriptionScopeIdentifier: 'servers-integration'
    parSubscriptionScope: '/apis/${parServerIntegrationApi.ApimApiName}'
    parTags: parTags
  }
}

module webApp 'modules/webApp.bicep' = {
  name: '${varDeploymentPrefix}-webApp'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.WebAppsResourceGroupName)

  params: {
    parLocation: parLocation
    parEnvironment: parEnvironment
    parKeyVaultName: varKeyVaultName
    parAppInsightsName: varAppInsightsName

    parStrategicServicesSubscriptionId: parStrategicServices.SubscriptionId
    parApiManagementResourceGroupName: parStrategicServices.ApiManagementResourceGroupName
    parApiManagementName: parStrategicServices.ApiManagementName
    parAppServicePlanName: parStrategicServices.AppServicePlanName
    parSqlServerResourceGroupName: parStrategicServices.SqlServerResourceGroupName
    parSqlServerName: parStrategicServices.SqlServerName

    parFrontDoorSubscriptionId: parFrontDoor.SubscriptionId
    parFrontDoorResourceGroupName: parFrontDoor.FrontDoorResourceGroupName
    parFrontDoorName: parFrontDoor.FrontDoorName

    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name

    parTags: parTags
  }
}

module sqlDatabase 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/sqldatabase:latest' = {
  name: '${varDeploymentPrefix}-sqlDatabase'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.SqlServerResourceGroupName)

  params: {
    parSqlServerName: parStrategicServices.SqlServerName
    parLocation: parLocation
    parDatabaseName: 'portal-web-${varEnvironmentUniqueId}'
    parSkuCapacity: 5
    parSkuName: 'Basic'
    parSkuTier: 'Basic'
    parTags: parTags
  }
}

module frontDoorEndpoint 'modules/frontDoorEndpoint.bicep' = {
  name: '${varDeploymentPrefix}-frontDoorEndpoint'
  scope: resourceGroup(parFrontDoor.SubscriptionId, parFrontDoor.FrontDoorResourceGroupName)

  params: {
    parDeploymentPrefix: varDeploymentPrefix
    parFrontDoorName: parFrontDoor.FrontDoorName
    parParentDnsName: parFrontDoor.ParentDnsName
    parDnsResourceGroupName: parDns.ResourceGroupName
    parWorkloadName: varWorkloadName
    parOriginHostName: webApp.outputs.outWebAppDefaultHostName
    parDnsZoneHostnamePrefix: varWorkloadName
    parCustomHostname: '${varWorkloadName}.${parDns.parParentDnsName}'
    parTags: parTags
  }
}
