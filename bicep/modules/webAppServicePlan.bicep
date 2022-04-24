targetScope = 'resourceGroup'

param parWebAppServicePlanName string
param parLocation string

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: parWebAppServicePlanName
  location: parLocation

  kind: 'linux'

  sku: {
    name: 'D1'
    tier: 'Shared'
  }
}

output outAppServicePlanId string = appServicePlan.id
