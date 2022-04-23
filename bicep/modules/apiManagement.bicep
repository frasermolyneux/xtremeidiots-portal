targetScope = 'resourceGroup'

param parRegion string
param parEnvironment string

resource apiManagement 'Microsoft.ApiManagement/service@2021-08-01' = {
  name: 'apim-portal-${parEnvironment}-${parRegion}'
  location: parRegion
  sku: {
    capacity: 0
    name: 'Consumption'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publisherEmail: 'admin@xtremeidiots.com'
    publisherName: 'XtremeIdiots'
  }
}
