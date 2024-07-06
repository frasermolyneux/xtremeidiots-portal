targetScope = 'subscription'

// Parameters
@description('The location of the resource group.')
param parLocation string

@description('The environment name (e.g. dev, tst, prd).')
param parEnvironment string

@description('The instance of the environment.')
param parInstance string

@description('The tags to apply to the resources.')
param parTags object

// Dynamic params from pipeline invocation
@description('The key vault create mode (e.g. recover, default).')
param parKeyVaultCreateMode string = 'recover'

// Variables
var varEnvironmentUniqueId = uniqueString('portal-web', parEnvironment, parInstance)
var varResourceGroupName = 'rg-portal-web-${parEnvironment}-${parLocation}-${parInstance}'
var varKeyVaultName = 'kv-${varEnvironmentUniqueId}-${parLocation}'

// Module Resources
resource defaultResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  tags: parTags

  properties: {}
}

module keyVault 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/keyvault:latest' = {
  name: '${deployment().name}-keyvault'
  scope: resourceGroup(defaultResourceGroup.name)

  params: {
    keyVaultName: varKeyVaultName
    keyVaultCreateMode: parKeyVaultCreateMode
    location: parLocation
    tags: parTags
  }
}
