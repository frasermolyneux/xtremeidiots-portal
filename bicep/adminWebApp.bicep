targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppServicePlanName string
param parAppInsightsName string
param parApiManagementName string
param parSqlServerName string

// Variables
var varAdminWebAppName = 'webapp-admin-portal-${parEnvironment}-${parLocation}-01'

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

resource sqlServer 'Microsoft.Sql/servers@2021-11-01-preview' existing = {
  name: parSqlServerName
}

// Module Resources
resource apiManagementSubscription 'Microsoft.ApiManagement/service/subscriptions@2021-08-01' = {
  name: '${apiManagement.name}-${varAdminWebAppName}-subscription'
  parent: apiManagement

  properties: {
    allowTracing: false
    displayName: varAdminWebAppName
    scope: '/apis'
  }
}

resource webAppApiMgmtKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${apiManagement.name}-${varAdminWebAppName}-apikey'
  parent: keyVault

  properties: {
    contentType: 'text/plain'
    value: apiManagementSubscription.properties.primaryKey
  }
}

resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: varAdminWebAppName
  location: parLocation
  kind: 'app'

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: appServicePlan.id

    httpsOnly: true

    siteConfig: {
      alwaysOn: true
      ftpsState: 'Disabled'

      netFrameworkVersion: 'v6.0'
      minTlsVersion: '1.2'

      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-instrumentationkey)'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-connectionstring)'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~2'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'apim-base-url'
          value: apiManagement.properties.gatewayUrl
        }
        {
          name: 'apim-subscription-key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varAdminWebAppName}-apikey)'
        }
        {
          name: 'sql-connection-string'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName};Authentication=Active Directory Default; Database=identitydb;'
        }
        {
          name: 'geolocation-baseurl'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=geolocation-baseurl)'
        }
        {
          name: 'geolocation-apikey'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=geolocation-apikey)'
        }
        {
          name: 'xtremeidiots-forums-base-url'
          value: 'https://www.xtremeidiots.com'
        }
        {
          name: 'xtremeidiots-forums-api-key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=xtremeidiots-forums-api-key)'
        }
      ]
    }
  }
}

resource webAppKeyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = {
  name: 'add'
  parent: keyVault

  properties: {
    accessPolicies: [
      {
        objectId: webApp.identity.principalId
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
