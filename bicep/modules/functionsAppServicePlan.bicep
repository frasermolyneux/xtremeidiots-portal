targetScope = 'resourceGroup'

param parFuncAppServicePlanName string
param parLocation string

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' = {
  name: parFuncAppServicePlanName
  location: parLocation
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

output outAppServicePlanId string = appServicePlan.id
