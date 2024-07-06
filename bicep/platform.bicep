targetScope = 'subscription'

// Parameters
@description('The location to deploy the resources')
param location string

@description('The environment for the resources')
param environment string

@description('The instance of the environment.')
param instance string

@description('The tags to apply to the resources.')
param tags object

// Dynamic params from pipeline invocation
@description('The key vault create mode (e.g. recover, default).')
param keyVaultCreateMode string = 'recover'

// Variables
var environmentUniqueId = uniqueString('portal-web', environment, instance)
var resourceGroupName = 'rg-portal-web-${environment}-${location}-${instance}'
var keyVaultName = 'kv-${environmentUniqueId}-${location}'

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
