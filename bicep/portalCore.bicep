targetScope = 'subscription'

param parLocation string
param parEnvironment string

var varResourceGroupName = 'rg-portal-${parEnvironment}-${parLocation}-01'
var varKeyVaultName = 'kv-portal-${parEnvironment}-${parLocation}-01'
var varLogWorkspaceName = 'log-portal-${parEnvironment}-${parLocation}-01'
var varAppInsightsName = 'ai-portal-${parEnvironment}-${parLocation}-01'
var varApimName = 'apim-portal-${parEnvironment}-${parLocation}-01'
var varAppServicePlanName = 'plan-portal-${parEnvironment}-${parLocation}-01'
var varServiceBusName = 'sb-portal-${parEnvironment}-${parLocation}-01'

resource portalResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: varResourceGroupName
  location: parLocation
  properties: {}
}

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault'
  scope: resourceGroup(portalResourceGroup.name)
  params: {
    parKeyVaultName: varKeyVaultName
    parLocation: parLocation
  }
}

module logging 'modules/logging.bicep' = {
  name: 'logging'
  scope: resourceGroup(portalResourceGroup.name)
  params: {
    parLogWorkspaceName: varLogWorkspaceName
    parAppInsightsName: varAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parLocation: parLocation
  }
}

module apiManagment 'modules/apiManagement.bicep' = {
  name: 'apiManagement'
  scope: resourceGroup(portalResourceGroup.name)
  params: {
    parApimName: varApimName
    parAppInsightsName: logging.outputs.outAppInsightsName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parLocation: parLocation
  }
}

module appServicePlan 'modules/appServicePlan.bicep' = {
  name: 'webAppServicePlan'
  scope: resourceGroup(portalResourceGroup.name)
  params: {
    parAppServicePlanName: varAppServicePlanName
    parLocation: parLocation
  }
}

module serviceBus 'modules/serviceBus.bicep' = {
  name: 'serviceBus'
  scope: resourceGroup(portalResourceGroup.name)
  params: {
    parServiceBusName: varServiceBusName
    parKeyVaultName: keyVault.outputs.outKeyVaultName
    parLocation: parLocation
  }
}
