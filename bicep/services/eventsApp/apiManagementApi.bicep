targetScope = 'resourceGroup'

// Parameters
param parApiManagementName string
param parFunctionAppName string
param parFunctionAppHostname string
param parEnvironment string
param parWorkloadSubscriptionId string
param parWorkloadResourceGroupName string
param parKeyVaultName string
param parAppInsightsName string

// Existing In-Scope Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
}

// Existing Out-Of-Scope Resources
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

// Module Resources
resource backendHostKeyNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: '${parFunctionAppName}-hostkey'
  parent: apiManagement

  properties: {
    displayName: '${parFunctionAppName}-hostkey'
    keyVault: {
      secretIdentifier: '${keyVault.properties.vaultUri}secrets/${parFunctionAppName}-hostkey'
    }
    secret: true
  }
}

resource apiBackend 'Microsoft.ApiManagement/service/backends@2021-08-01' = {
  name: parFunctionAppName
  parent: apiManagement

  properties: {
    title: parFunctionAppName
    description: parFunctionAppName
    url: 'https://${parFunctionAppHostname}/api/'
    protocol: 'http'
    properties: {}

    credentials: {
      query: {
        code: [
          '{{${backendHostKeyNamedValue.name}}}'
        ]
      }
    }

    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource apiActiveBackendNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'events-api-active-backend'
  parent: apiManagement

  properties: {
    displayName: 'events-api-active-backend'
    value: apiBackend.name
    secret: false
  }
}

resource apiAudienceNamedValue 'Microsoft.ApiManagement/service/namedValues@2021-08-01' = {
  name: 'events-api-audience'
  parent: apiManagement

  properties: {
    displayName: 'events-api-audience'
    value: 'api://portal-events-api-${parEnvironment}'
    secret: false
  }
}

resource api 'Microsoft.ApiManagement/service/apis@2021-08-01' = {
  name: 'events-api'
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
    value: loadTextContent('./../../../.azure-pipelines/api-definitions/Events.openapi+json.json')
  }
}

resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-08-01' = {
  name: 'policy'
  parent: api
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
    apiActiveBackendNamedValue
    apiAudienceNamedValue
  ]
}

resource apiDiagnostics 'Microsoft.ApiManagement/service/apis/diagnostics@2021-08-01' = {
  name: 'applicationinsights'
  parent: api

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
