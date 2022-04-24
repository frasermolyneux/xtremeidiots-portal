targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parFuncAppServicePlanName string
param parAppInsightsName string
param parApiManagementName string
param parServiceBusName string
param parEventsApiAppId string

// Variables
var varEventsFuncAppName = 'fn-events-portal-${parEnvironment}-${parLocation}-01'

// Existing Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
}

resource funcAppServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parFuncAppServicePlanName
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
resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: 'sa${uniqueString(resourceGroup().id)}${parEnvironment}'
  location: parLocation
  kind: 'StorageV2'

  sku: {
    name: 'Standard_LRS'
  }
}

resource functionApp 'Microsoft.Web/sites@2020-06-01' = {
  name: varEventsFuncAppName
  location: parLocation
  kind: 'functionapp'

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: funcAppServicePlan.id

    httpsOnly: true

    siteConfig: {
      ftpsState: 'Disabled'

      appSettings: [
        {
          'name': 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          'value': '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-connectionstring)'
        }
        {
          'name': 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'
          'value': '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=portal-events-api-prd-clientsecret)'
        }
        {
          'name': 'service-bus-connection-string'
          'value': '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${serviceBus}-connectionstring)'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          'name': 'FUNCTIONS_EXTENSION_VERSION'
          'value': '~4'
        }
        {
          'name': 'FUNCTIONS_WORKER_RUNTIME'
          'value': 'dotnet'
        }
        {
          'name': 'WEBSITE_RUN_FROM_PACKAGE'
          'value': '1'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        // WEBSITE_CONTENTSHARE will also be auto-generated - https://docs.microsoft.com/en-us/azure/azure-functions/functions-app-settings#website_contentshare
      ]
    }
  }
}

resource functionAppAuthSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'authsettingsV2'
  kind: 'string'
  parent: functionApp

  properties: {
    globalValidation: {
      requireAuthentication: true
      unauthenticatedClientAction: 'Return403'
    }

    httpSettings: {
      requireHttps: true
    }

    identityProviders: {
      azureActiveDirectory: {
        enabled: true

        registration: {
          clientId: parEventsApiAppId
          clientSecretSettingName: 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'
          openIdIssuer: 'https://sts.windows.net/${tenant().tenantId}/v2.0'
        }

        validation: {
          allowedAudiences: [
            'api://portal-events-api-${parEnvironment}'
          ]
        }
      }
    }
  }
}

resource functionAppKeyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2021-11-01-preview' = {
  name: 'add'
  parent: keyVault

  properties: {
    accessPolicies: [
      {
        objectId: functionApp.identity.principalId
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

resource functionHostKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${functionApp.name}-hostkey'
  parent: keyVault

  properties: {
    contentType: 'text/plain'
    value: listkeys('${functionApp.id}/host/default', '2016-08-01').functionKeys.default
  }
}

resource backendHostKeyNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: functionHostKeySecret.name
  parent: apiManagement

  properties: {
    displayName: functionHostKeySecret.name
    keyVault: {
      secretIdentifier: functionHostKeySecret.properties.secretUri
    }
    secret: true
  }
}

resource apiBackend 'Microsoft.ApiManagement/service/backends@2021-08-01' = {
  name: functionApp.name
  parent: apiManagement

  properties: {
    title: functionApp.name
    description: functionApp.name
    url: 'https://${functionApp.properties.defaultHostName}/api/'
    protocol: 'http'
    properties: {}

    credentials: {
      query: {
        code: [
          '{{${functionHostKeySecret.name}}}'
        ]
      }
    }

    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource eventsApiActiveBackendNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'events-api-active-backend'
  parent: apiManagement

  properties: {
    displayName: 'events-api-active-backend'
    value: apiBackend.name
    secret: false
  }
}

resource eventsApiAudienceNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'events-api-audience'
  parent: apiManagement

  properties: {
    displayName: 'events-api-audience'
    value: 'api://portal-events-api-${parEnvironment}'
    secret: false
  }
}

resource eventsApi 'Microsoft.ApiManagement/service/apis@2021-08-01' = {
  name: 'eventsApi'
  parent: apiManagement

  properties: {
    apiRevision: '1.0'
    apiType: 'http'
    type: 'http'

    description: 'API for player/server events'
    displayName: 'Events API'
    path: 'events'

    protocols: [
      'https'
    ]

    subscriptionRequired: true
    subscriptionKeyParameterNames: {
      header: 'Ocp-Apim-Subscription-Key'
    }

    format: 'openapi+json'
    value: loadTextContent('./../api-definitions/Events.openapi+json.json')
  }
}

resource eventsApiPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-08-01' = {
  name: 'policy'
  parent: eventsApi
  properties: {
    format: 'xml'
    value: '''
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="{{events-api-active-backend}}" />
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
          <openid-config url="{{tenant-login-url}}{{tenant-id}}/v2.0/.well-known/openid-configuration" />
          <audiences>
              <audience>{{events-api-audience}}</audience>
          </audiences>
          <issuers>
              <issuer>https://sts.windows.net/{{tenant-id}}/</issuer>
          </issuers>
          <required-claims>
              <claim name="roles" match="any">
                  <value>ServerEventGenerator</value>
                  <value>PlayerEventGenerator</value>
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
    eventsApiActiveBackendNamedValue
    eventsApiAudienceNamedValue
  ]
}