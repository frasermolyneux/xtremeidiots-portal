targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppServicePlanName string
param parAppInsightsName string
param parApiManagementName string
param parServiceBusName string

// Variables
var varTempAdminWebAppName = 'legacy-web-app'

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-08-01' existing = {
  name: parApiManagementName
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: parServiceBusName
}

// Module Resources
resource apiManagementSubscription 'Microsoft.ApiManagement/service/subscriptions@2021-08-01' = {
  name: 'apiManagementSubscription'
  parent: apiManagement

  properties: {
    allowTracing: false
    displayName: varTempAdminWebAppName
    scope: '/apis'
  }
}

resource webAppApiMgmtKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${apiManagement.name}-${varTempAdminWebAppName}-apikey'
  parent: keyVault

  properties: {
    contentType: 'text/plain'
    value: apiManagementSubscription.properties.primaryKey
  }
}
