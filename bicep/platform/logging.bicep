targetScope = 'resourceGroup'

// Parameters
param parLogWorkspaceName string
param parAppInsightsName string
param parKeyVaultName string
param parLocation string

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

// Module Resources
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: parLogWorkspaceName
  location: parLocation
  properties: {
    retentionInDays: 90
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: parAppInsightsName
  location: parLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource appInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${appInsights.name}-connectionstring'
  parent: keyVault
  properties: {
    contentType: 'text/plain'
    value: appInsights.properties.ConnectionString
  }
}

resource appInsightsInstrumentationKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${appInsights.name}-instrumentationkey'
  parent: keyVault
  properties: {
    contentType: 'text/plain'
    value: appInsights.properties.InstrumentationKey
  }
}

output outAppInsightsId string = appInsights.id
output outAppInsightsName string = appInsights.name
output outAppInsightsConnectionString string = appInsights.properties.ConnectionString
