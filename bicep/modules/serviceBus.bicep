targetScope = 'resourceGroup'

// Parameters
param parServiceBusName string
param parLocation string
param parKeyVaultName string

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: parServiceBusName
  location: parLocation

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
