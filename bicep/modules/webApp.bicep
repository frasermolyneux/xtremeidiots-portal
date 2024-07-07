targetScope = 'resourceGroup'

// Parameters
@description('The name of the web app.')
param webAppName string

@description('The environment for the resources')
param environment string

@description('The environment unique id (e.g. 1234).')
param environmentUniqueId string

@description('The location to deploy the resources')
param location string = resourceGroup().location

@description('A reference to the key vault resource')
param keyVaultRef object

@description('A reference to the app insights resource')
param appInsightsRef object

@description('A reference to the app service plan resource')
param appServicePlanRef object

@description('A reference to the api management resource')
param apiManagementRef object

@description('A reference to the sql server resource')
param sqlServerRef object

@description('The repository api object.')
param repositoryApi object

@description('The servers integration api object.')
param serversIntegrationApi object

@description('The geo location api object.')
param geoLocationApi object

@description('The dns configuration object')
param dns object

@description('The tags to apply to the resources.')
param tags object

// Existing Out-Of-Scope Resources
resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: apiManagementRef.Name
  scope: resourceGroup(apiManagementRef.SubscriptionId, apiManagementRef.ResourceGroupName)
}

resource sqlServer 'Microsoft.Sql/servers@2021-11-01-preview' existing = {
  name: sqlServerRef.Name
  scope: resourceGroup(sqlServerRef.SubscriptionId, sqlServerRef.ResourceGroupName)
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: appServicePlanRef.Name
  scope: resourceGroup(appServicePlanRef.SubscriptionId, appServicePlanRef.ResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsRef.Name
  scope: resourceGroup(appInsightsRef.SubscriptionId, appInsightsRef.ResourceGroupName)
}

// Module Resources
module repositoryApimSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: 'repositoryApimSubscription'
  scope: resourceGroup(apiManagementRef.SubscriptionId, apiManagementRef.ResourceGroupName)

  params: {
    apiManagementName: apiManagement.name
    workloadName: webAppName
    apiScope: repositoryApi.ApimApiName
    keyVaultRef: keyVaultRef
    tags: tags
  }
}

module serversApimSubscription 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/apimanagementsubscription:latest' = {
  name: 'serversApimSubscription'
  scope: resourceGroup(apiManagementRef.SubscriptionId, apiManagementRef.ResourceGroupName)

  params: {
    apiManagementName: apiManagement.name
    workloadName: webAppName
    apiScope: serversIntegrationApi.ApimApiName
    keyVaultRef: keyVaultRef
    tags: tags
  }
}

resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: webAppName
  location: location
  kind: 'app'
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: appServicePlan.id

    httpsOnly: true

    siteConfig: {
      ftpsState: 'Disabled'

      alwaysOn: true
      linuxFxVersion: 'DOTNETCORE|8.0'
      netFrameworkVersion: 'v8.0'
      minTlsVersion: '1.2'

      healthCheckPath: '/api/health'

      appSettings: [
        {
          name: 'READ_ONLY_MODE'
          value: (environment == 'prd') ? 'true' : 'false'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
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
          name: 'apim_base_url'
          value: apiManagement.properties.gatewayUrl
        }
        {
          name: 'portal_repository_apim_subscription_key'
          value: '@Microsoft.KeyVault(SecretUri=${repositoryApimSubscription.outputs.primaryKeySecretRef.secretUri})'
        }
        {
          name: 'portal_servers_apim_subscription_key'
          value: '@Microsoft.KeyVault(SecretUri=${serversApimSubscription.outputs.primaryKeySecretRef.secretUri})'
        }
        {
          name: 'geolocation_base_url'
          value: geoLocationApi.BaseUrl
        }
        {
          name: 'geolocation_apim_subscription_key_primary'
          value: '@Microsoft.KeyVault(SecretUri=${geoLocationApi.KeyVaultPrimaryRef})'
        }
        {
          name: 'geolocation_apim_subscription_key_secondary'
          value: '@Microsoft.KeyVault(SecretUri=${geoLocationApi.KeyVaultSecondaryRef})'
        }
        {
          name: 'repository_api_application_audience'
          value: repositoryApi.ApplicationAudience
        }
        {
          name: 'servers_api_application_audience'
          value: serversIntegrationApi.ApplicationAudience
        }
        {
          name: 'geolocation_api_application_audience'
          value: geoLocationApi.ApplicationAudience
        }
        {
          name: 'sql_connection_string'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName};Authentication=Active Directory Default; Database=portal-web-${environmentUniqueId};'
        }
        {
          name: 'xtremeidiots_forums_base_url'
          value: 'https://www.xtremeidiots.com'
        }
        {
          name: 'xtremeidiots_forums_api_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultRef.Name};SecretName=xtremeidiots-forums-api-key)'
        }
        {
          name: 'xtremeidiots_auth_client_id'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultRef.Name};SecretName=xtremeidiots-auth-client-id)'
        }
        {
          name: 'xtremeidiots_auth_client_secret'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultRef.Name};SecretName=xtremeidiots-auth-client-secret)'
        }
        {
          name: 'repository_api_path_prefix'
          value: repositoryApi.ApimPathPrefix
        }
        {
          name: 'servers_api_path_prefix'
          value: serversIntegrationApi.ApimPathPrefix
        }
        {
          name: 'geolocation_api_path_prefix'
          value: geoLocationApi.ApimPathPrefix
        }
        {
          name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'DiagnosticServices_EXTENSION_VERSION'
          value: '~3'
        }
      ]
    }
  }
}

module webTest 'br:acrty7og2i6qpv3s.azurecr.io/bicep/modules/webtest:latest' = {
  name: '${deployment().name}-webtest'
  scope: resourceGroup(appInsightsRef.SubscriptionId, appInsightsRef.ResourceGroupName)

  params: {
    workloadName: webApp.name
    testUrl: 'https://${webApp.properties.defaultHostName}/api/health'
    appInsightsRef: appInsightsRef
    location: location
    tags: tags
  }
}

module webAppDns 'dnsWebApp.bicep' = {
  name: '${deployment().name}-dns'
  scope: resourceGroup(dns.SubscriptionId, dns.ResourceGroupName)

  params: {
    dns: dns
    webAppHostname: webApp.properties.defaultHostName
    domainAuthCode: webApp.properties.customDomainVerificationId
    tags: tags
  }
}

resource customDomain 'Microsoft.Web/sites/hostNameBindings@2023-01-01' = {
  name: '${dns.Subdomain}.${dns.Domain}'
  parent: webApp

  properties: {
    siteName: webApp.name
  }

  dependsOn: [
    webAppDns
  ]
}

// Outputs
output webAppDefaultHostName string = webApp.properties.defaultHostName
output webAppIdentityPrincipalId string = webApp.identity.principalId
output webAppName string = webApp.name
