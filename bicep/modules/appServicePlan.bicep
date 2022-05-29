targetScope = 'resourceGroup'

param parAppServicePlanName string
param parLocation string

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: parAppServicePlanName
  location: parLocation

  sku: {
    name: 'S1'
    tier: 'Standard'
  }
}

output outAppServicePlanId string = appServicePlan.id
