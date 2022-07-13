targetScope = 'resourceGroup'

// Parameters
param parServiceBusName string
param parLocation string
param parKeyVaultName string
param parTags object

// Existing In-Scope Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: parServiceBusName
  location: parLocation
  tags: parTags

  sku: {
    name: 'Basic'
    tier: 'Basic'
  }

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    alternateName: 'string'
  }
}

resource serviceBusConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${serviceBus.name}-connectionstring'
  parent: keyVault
  tags: parTags

  properties: {
    contentType: 'text/plain'
    value: 'Endpoint=sb://${serviceBus.name}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${listKeys('${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBus.apiVersion).primaryKey}'
  }
}

// Outputs
output outServiceBusName string = serviceBus.name
