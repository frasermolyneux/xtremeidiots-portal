targetScope = 'subscription'

// Parameters
@description('The location to deploy the resources')
param location string

@description('The environment for the resources')
param environment string

@description('The instance of the environment.')
param instance string

@description('The api management resource name')
param apiManagementName string

@description('The name of the SQL Server')
param sqlServerName string

@description('The DNS configuration.')
param dns object

@description('The repository API configuration.')
param repositoryApi object

@description('The servers integration API configuration.')
param serversIntegrationApi object

@description('The geo location API configuration.')
param geoLocationApi object

@description('The tags to apply to the resources.')
param tags object

// Dynamic params from pipeline invocation
@description('The key vault create mode (e.g. recover, default).')
param keyVaultCreateMode string = 'recover'

// Variables
var environmentUniqueId = uniqueString('portal-web', environment, instance)
var resourceGroupName = 'rg-portal-web-${environment}-${location}-${instance}'
var adminWebAppName = 'app-portal-web-${environment}-${location}-${instance}-${environmentUniqueId}'
var keyVaultName = 'kv-${environmentUniqueId}-${location}'

// Resource References
var appInsightsRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${environment}-${location}-${instance}'
  Name: 'ai-portal-core-${environment}-${location}-${instance}'
}

var appServicePlanRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${environment}-${location}-${instance}'
  Name: 'asp-portal-core-${environment}-${location}-${instance}'
}

var apiManagementRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${environment}-${location}-${instance}'
  Name: apiManagementName
}

var sqlServerRef = {
  SubscriptionId: subscription().subscriptionId
  ResourceGroupName: 'rg-portal-core-${environment}-${location}-${instance}'
  Name: sqlServerName
}

@description('https://learn.microsoft.com/en-gb/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: tags

  properties: {}
}

module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${deployment().name}-keyvault'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    keyVaultName: keyVaultName
    keyVaultCreateMode: keyVaultCreateMode
    location: location
    tags: tags
  }
}

module webApp 'modules/webApp.bicep' = {
  name: '${deployment().name}-webapp'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    webAppName: adminWebAppName
    environment: environment
    environmentUniqueId: environmentUniqueId
    location: location

    keyVaultRef: keyVault.outputs.keyVaultRef
    appInsightsRef: appInsightsRef
    appServicePlanRef: appServicePlanRef
    apiManagementRef: apiManagementRef
    sqlServerRef: sqlServerRef

    repositoryApi: repositoryApi
    serversIntegrationApi: serversIntegrationApi
    geoLocationApi: geoLocationApi

    dns: dns
    tags: tags
  }
}

module webAppKeyVaultRoleAssignment 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvaultroleassignment:latest' = {
  name: '${deployment().name}-webappkvrole'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    keyVaultName: keyVault.outputs.keyVaultRef.name
    principalId: webApp.outputs.webAppIdentityPrincipalId
    roleDefinitionId: keyVaultSecretUserRoleDefinition.id
  }
}

module sqlDatabase 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/sqldatabase:latest' = {
  name: '${deployment().name}-sqldb'
  scope: resourceGroup(sqlServerRef.SubscriptionId, sqlServerRef.ResourceGroupName)

  params: {
    sqlServerName: sqlServerRef.Name
    databaseName: 'portal-web-${environmentUniqueId}'
    skuCapacity: 5
    skuName: 'Basic'
    skuTier: 'Basic'
    location: location
    tags: tags
  }
}

// Outputs
output webAppIdentityPrincipalId string = webApp.outputs.webAppIdentityPrincipalId
output webAppName string = webApp.outputs.webAppName
