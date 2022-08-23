targetScope = 'resourceGroup'

// Parameters
param parDeploymentPrefix string
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
  name: 'sasyncappdata${parEnvironment}'
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
  name: 'ban-files'
  parent: storageAccountBlobServices

  properties: {
    publicAccess: 'Blob'
  }
}

module keyVaultSecret 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultsecret:latest' = {
  name: '${parDeploymentPrefix}-${storageAccount.name}-keyVaultSecret'

  params: {
    parKeyVaultName: keyVault.name
    parSecretName: '${storageAccount.name}-connectionstring'
    parSecretValue: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
    parTags: parTags
  }
}

// Outputs
output outStorageAccountName string = storageAccount.name
