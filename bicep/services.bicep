targetScope = 'resourceGroup'

@description('The location of the resource group.')
param parLocation string

@description('The environment name (e.g. dev, tst, prd).')
param parEnvironment string

@description('The instance of the environment.')
param parInstance string

@description('The name of the API Management')
param parApiManagementName string

@description('The name of the SQL Server')
param parSqlServerName string

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
var varAdminWebAppName = 'app-portal-web-${parEnvironment}-${parLocation}-${parInstance}-${varEnvironmentUniqueId}'

// External Resource References
var varAppInsightsRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
  Name: 'ai-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
}

var varKeyVaultRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: resourceGroup().name
  Name: 'kv-${varEnvironmentUniqueId}-${parLocation}'
}

var varAppServicePlanRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
  Name: 'asp-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
}

var varApiManagementRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
  Name: parApiManagementName
}

var varSqlServerRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${parEnvironment}-${parLocation}-${parInstance}'
  Name: parSqlServerName
}

// Existing Out-Of-Scope Resources
@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

// Module Resources
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
  scope: resourceGroup(varSqlServerRef.SubscriptionId, varSqlServerRef.ResourceGroupName)

  params: {
    parSqlServerName: varSqlServerRef.Name
    parLocation: parLocation
    parDatabaseName: 'portal-web-${varEnvironmentUniqueId}'
    parSkuCapacity: 5
    parSkuName: 'Basic'
    parSkuTier: 'Basic'
    parTags: parTags
  }
}

// Outputs
output outWebAppIdentityPrincipalId string = webApp.outputs.outWebAppIdentityPrincipalId
output outWebAppName string = webApp.outputs.outWebAppName
