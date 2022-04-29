targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppServicePlanName string
param parAppInsightsName string
param parApiManagementName string
param parRepositoryApiAppId string

// Variables
var varRepositoryWebAppName = 'webapi-repository-portal-${parEnvironment}-${parLocation}-01'

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

// Module Resources
resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: varRepositoryWebAppName
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
          'name': 'APPINSIGHTS_INSTRUMENTATIONKEY'
          'value': '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-instrumentationkey)'
        }
        {
          'name': 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          'value': '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-connectionstring)'
        }
        {
          'name': 'ApplicationInsightsAgent_EXTENSION_VERSION'
          'value': '~2'
        }
        {
          'name': 'ASPNETCORE_ENVIRONMENT'
          'value': 'Production'
        }
        {
          'name': 'AzureAd:TenantId'
          'value': tenant().tenantId
        }
        {
          'name': 'AzureAd:ClientId'
          'value': parRepositoryApiAppId
        }
        {
          'name': 'AzureAd:ClientSecret'
          'value': '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=portal-repository-api-prd-clientsecret)'
        }
        {
          'name': 'AzureAd:Audience'
          'value': 'api://portal-repository-api-${parEnvironment}'
        }
        {
          'name': 'sql-connection-string'
          'value': '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=legacy-sql-connection-string)'
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

resource apiBackend 'Microsoft.ApiManagement/service/backends@2021-08-01' = {
  name: webApp.name
  parent: apiManagement

  properties: {
    title: webApp.name
    description: webApp.name
    url: 'https://${webApp.properties.defaultHostName}/api/'
    protocol: 'http'
    properties: {}

    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource repositoryApiActiveBackendNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'repository-api-active-backend'
  parent: apiManagement

  properties: {
    displayName: 'repository-api-active-backend'
    value: apiBackend.name
    secret: false
  }
}

resource repositoryApiAudienceNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'repository-api-audience'
  parent: apiManagement

  properties: {
    displayName: 'repository-api-audience'
    value: 'api://portal-repository-api-${parEnvironment}'
    secret: false
  }
}

resource repositoryApi 'Microsoft.ApiManagement/service/apis@2021-08-01' = {
  name: 'repositoryApi'
  parent: apiManagement

  properties: {
    apiRevision: '1.0'
    apiType: 'http'
    type: 'http'

    description: 'API for repository layer'
    displayName: 'Repository API'
    path: 'repository'

    protocols: [
      'https'
    ]

    subscriptionRequired: true
    subscriptionKeyParameterNames: {
      header: 'Ocp-Apim-Subscription-Key'
    }

    format: 'openapi+json'
    value: loadTextContent('./../api-definitions/Repository.openapi+json.json')
  }
}

resource repositoryApiPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-08-01' = {
  name: 'policy'
  parent: repositoryApi
  properties: {
    format: 'xml'
    value: '''
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="{{repository-api-active-backend}}" />
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
          <openid-config url="{{tenant-login-url}}{{tenant-id}}/v2.0/.well-known/openid-configuration" />
          <audiences>
              <audience>{{repository-api-audience}}</audience>
          </audiences>
          <issuers>
              <issuer>https://sts.windows.net/{{tenant-id}}/</issuer>
          </issuers>
          <required-claims>
              <claim name="roles" match="any">
                <value>ServiceAccount</value>
              </claim>
          </required-claims>
      </validate-jwt>
  </inbound>
  <backend>
      <forward-request />
  </backend>
  <outbound>
      <base/>
      <cache-store duration="3600" />
  </outbound>
  <on-error />
</policies>'''
  }

  dependsOn: [
    repositoryApiActiveBackendNamedValue
    repositoryApiAudienceNamedValue
  ]
}

resource repositoryApiDiagnostics 'Microsoft.ApiManagement/service/apis/diagnostics@2021-08-01' = {
  name: 'applicationinsights'
  parent: repositoryApi

  properties: {
    alwaysLog: 'allErrors'

    httpCorrelationProtocol: 'W3C'
    logClientIp: true
    loggerId: resourceId('Microsoft.ApiManagement/service/loggers', apiManagement.name, appInsights.name)
    operationNameFormat: 'Name'

    sampling: {
      percentage: 100
      samplingType: 'fixed'
    }

    verbosity: 'information'
  }
}
