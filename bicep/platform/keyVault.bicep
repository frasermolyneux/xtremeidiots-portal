targetScope = 'resourceGroup'

param parKeyVaultName string
param parLocation string

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: parKeyVaultName
  location: parLocation

  properties: {
    accessPolicies: []
    createMode: 'recover'

    enablePurgeProtection: true
    enableRbacAuthorization: false
    enableSoftDelete: true

    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }

    sku: {
      family: 'A'
      name: 'standard'
    }

    softDeleteRetentionInDays: 30

    tenantId: tenant().tenantId
  }
}

output outKeyVaultName string = keyVault.name
