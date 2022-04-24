targetScope = 'resourceGroup'

// Parameters
param parApimName string
param parLocation string
param parAppInsightsName string
param parKeyVaultName string

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
}

resource appInsightsInstrumentationKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' existing = {
  name: '${appInsights.name}-instrumentationkey'
}

// Module Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-08-01' = {
  name: parApimName
  location: parLocation

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

resource apiManagementKeyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = {
  name: 'add'
  parent: keyVault

  properties: {
    accessPolicies: [
      {
        objectId: apiManagement.identity.principalId
        permissions: {
          certificates: []
          keys: []
          secrets: [
            'get'
          ]
          storage: []
        }
        tenantId: tenant().tenantId
      }
    ]
  }
}

resource appInsightsInstrumentationKeyNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: appInsightsInstrumentationKeySecret.name
  parent: apiManagement

  properties: {
    displayName: appInsightsInstrumentationKeySecret.name
    keyVault: {
      secretIdentifier: '${keyVault.properties.vaultUri}secrets/${appInsightsInstrumentationKeySecret.name}'
    }
    secret: true
  }
}

resource tenantIdNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'tenant-id'
  parent: apiManagement

  properties: {
    displayName: 'tenant-id'
    value: tenant().tenantId
    secret: false
  }
}

resource tenantLoginUrlNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'tenant-login-url'
  parent: apiManagement

  properties: {
    displayName: 'tenant-login-url'
    value: environment().authentication.loginEndpoint
    secret: false
  }
}

resource apiManagementLogger 'Microsoft.ApiManagement/service/loggers@2021-08-01' = {
  name: appInsights.name
  parent: apiManagement

  properties: {
    credentials: {
      instrumentationKey: '{{${appInsightsInstrumentationKeySecret.name}}}'
    }
    loggerType: 'applicationInsights'
    resourceId: appInsights.id
  }
}

output outApimId string = apiManagement.id
output outApimName string = apiManagement.name
