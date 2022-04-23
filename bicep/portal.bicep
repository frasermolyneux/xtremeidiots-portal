targetScope = 'subscription'

param parRegion string
param parEnvironment string

resource portalResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-portal-${parEnvironment}-${parRegion}'
  location: parRegion
  properties: {}
}

module apiManagment 'modules/apiManagement.bicep' = {
  name: 'apiManagement'
  scope: resourceGroup(portalResourceGroup.name)
  params: {
    parRegion: parRegion
    parEnvironment: parEnvironment
  }
}
