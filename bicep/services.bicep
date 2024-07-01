targetScope = 'resourceGroup'

@description('The location of the resource group.')
param parLocation string

@description('The environment name (e.g. dev, tst, prd).')
param parEnvironment string

@description('The instance of the environment.')
param parInstance string

@description('The front door configuration.')
param parFrontDoorRef object

@description('The DNS configuration.')
param parDns object

@description('The strategic services configuration.')
param parStrategicServices object

@description('The repository API configuration.')
param parRepositoryApi object

@description('The servers integration API configuration.')
param parServersIntegrationApi object

@description('The geo location API configuration.')
param parGeoLocationApi object

@description('The tags to apply to the resources.')
param parTags object

// Variables
var varEnvironmentUniqueId = uniqueString('portal-web', parEnvironment, parInstance)
var varWorkloadName = 'app-portal-web-${parEnvironment}-${parInstance}-${varEnvironmentUniqueId}'
var varAdminWebAppName = 'app-portal-web-${parEnvironment}-${parLocation}-${parInstance}-${varEnvironmentUniqueId}'

// External Resource References
var varAppInsightsRef = {
  Name: 'ai-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
}

var varKeyVaultRef = {
  Name: 'kv-${varEnvironmentUniqueId}-${parLocation}'
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: resourceGroup().name
}

var varAppServicePlanRef = {
  Name: 'asp-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
}

var varApiManagementRef = {
  Name: parStrategicServices.ApiManagementName
  SubscriptionId: parStrategicServices.SubscriptionId
  ResourceGroupName: parStrategicServices.ApiManagementResourceGroupName
}

var varSqlServerRef = {
  Name: parStrategicServices.SqlServerName
  SubscriptionId: parStrategicServices.SubscriptionId
  ResourceGroupName: parStrategicServices.SqlServerResourceGroupName
}

// Existing Out-Of-Scope Resources
@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

// Module Resources
module repositoryApiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${deployment().name}-repoapimsubscription'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.ApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: deployment().name
    parApiManagementName: parStrategicServices.ApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: varKeyVaultRef.Name
    parSubscriptionScopeIdentifier: 'repository'
    parSubscriptionScope: '/apis/${parRepositoryApi.ApimApiName}'
    parTags: parTags
  }
}

module serversApiManagementSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: '${deployment().name}-serversapimsubscription'
  scope: resourceGroup(parStrategicServices.SubscriptionId, parStrategicServices.ApiManagementResourceGroupName)

  params: {
    parDeploymentPrefix: deployment().name
    parApiManagementName: parStrategicServices.ApiManagementName
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varAdminWebAppName
    parKeyVaultName: varKeyVaultRef.Name
    parSubscriptionScopeIdentifier: 'servers-integration'
    parSubscriptionScope: '/apis/${parServersIntegrationApi.ApimApiName}'
    parTags: parTags
  }
}

module webApp 'modules/webApp.bicep' = {
  name: '${deployment().name}-webapp'

  params: {
    parWebAppName: varAdminWebAppName
    parEnvironment: parEnvironment
    parEnvironmentUniqueId: varEnvironmentUniqueId
    parLocation: parLocation

    parKeyVaultRef: varKeyVaultRef
    parAppInsightsRef: varAppInsightsRef
    parAppServicePlanRef: varAppServicePlanRef
    parApiManagementRef: varApiManagementRef
    parSqlServerRef: varSqlServerRef
    parFrontDoorRef: parFrontDoorRef

    parRepositoryApi: parRepositoryApi
    parServersIntegrationApi: parServersIntegrationApi
    parGeoLocationApi: parGeoLocationApi

    parDns: parDns
    parTags: parTags
  }
}

module webAppKeyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${deployment().name}-webappkvrole'

  params: {
    parKeyVaultName: varKeyVaultRef.Name
    parRoleDefinitionId: keyVaultSecretUserRoleDefinition.id
    parPrincipalId: webApp.outputs.outWebAppIdentityPrincipalId
  }
}

module sqlDatabase 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/sqldatabase:latest' = {
  name: '${deployment().name}-sqldb'
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
  name: '${deployment().name}-fdendpoint'
  scope: resourceGroup(parFrontDoorRef.SubscriptionId, parFrontDoorRef.ResourceGroupName)

  params: {
    parFrontDoorName: parFrontDoorRef.Name
    parParentDnsName: parDns.ParentDnsName
    parDnsResourceGroupName: parDns.DnsResourceGroupName
    parWorkloadName: varWorkloadName
    parOriginHostName: webApp.outputs.outWebAppDefaultHostName
    parDnsZoneHostnamePrefix: varWorkloadName
    parCustomHostname: '${varWorkloadName}.${parDns.ParentDnsName}'
    parTags: parTags
  }
}

// Outputs
output outWebAppIdentityPrincipalId string = webApp.outputs.outWebAppIdentityPrincipalId
output outWebAppName string = webApp.outputs.outWebAppName
