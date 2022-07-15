targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parTags object

// Existing Out-Of-Scope Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: 'saptlrepoappdata${parEnvironment}'
  location: parLocation
  kind: 'StorageV2'
  tags: parTags

  sku: {
    name: 'Standard_LRS'
  }
}

resource storageAccountBlobServices 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' = {
  name: 'default'
  parent: storageAccount
  properties: {}
}

resource repositoryApiMapImageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: 'map-images'
  parent: storageAccountBlobServices
  properties: {
    publicAccess: 'Blob'
  }
}

resource repositoryApiDemosContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: 'demos'
  parent: storageAccountBlobServices
  properties: {
    publicAccess: 'Blob'
  }
}

// Outputs
output outStorageAccountName string = storageAccount.name
