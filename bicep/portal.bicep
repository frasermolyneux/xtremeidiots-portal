targetScope = 'subscription'

param parRegion string
param parEnvironment string

resource symbolicname 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-portal-${parEnvironment}-${parRegion}'
  location: parRegion
  properties: {}
}
