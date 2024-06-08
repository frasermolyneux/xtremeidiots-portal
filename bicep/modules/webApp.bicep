targetScope = 'resourceGroup'

// Parameters
@description('The name of the web app.')
param parWebAppName string

@description('The environment name (e.g. dev, tst, prd).')
param parEnvironment string

@description('The environment unique id (e.g. 1234).')
param parEnvironmentUniqueId string

@description('The location of the resource group.')
param parLocation string

// -- References
@description('The key vault reference')
param parKeyVaultRef object

@description('The app insights reference')
param parAppInsightsRef object

@description('The app service plan reference')
param parAppServicePlanRef object

@description('The api management reference')
param parApiManagementRef object

@description('The sql server reference')
param parSqlServerRef object

@description('The front door reference')
param parFrontDoorRef object

// -- Apis

@description('The repository api object.')
param parRepositoryApi object

@description('The servers integration api object.')
param parServersIntegrationApi object

@description('The geo location api object.')
param parGeoLocationApi object

// -- Common
@description('The tags to apply to the resources.')
param parTags object

// Existing Out-Of-Scope Resources
resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' existing = {
  name: parFrontDoorRef.Name
  scope: resourceGroup(parFrontDoorRef.SubscriptionId, parFrontDoorRef.ResourceGroupName)
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementRef.Name
  scope: resourceGroup(parApiManagementRef.SubscriptionId, parApiManagementRef.ResourceGroupName)
}

resource sqlServer 'Microsoft.Sql/servers@2021-11-01-preview' existing = {
  name: parSqlServerRef.Name
  scope: resourceGroup(parSqlServerRef.SubscriptionId, parSqlServerRef.ResourceGroupName)
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanRef.Name
  scope: resourceGroup(parAppServicePlanRef.SubscriptionId, parAppServicePlanRef.ResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsRef.Name
  scope: resourceGroup(parAppInsightsRef.SubscriptionId, parAppInsightsRef.ResourceGroupName)
}

// Module Resources
resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: parWebAppName
  location: parLocation
  kind: 'app'
  tags: parTags

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

      ipSecurityRestrictions: [
        {
          ipAddress: 'AzureFrontDoor.Backend'
          action: 'Allow'
          tag: 'ServiceTag'
          priority: 1000
          name: 'RestrictToFrontDoor'
          headers: {
            'x-azure-fdid': [
              frontDoor.properties.frontDoorId
            ]
          }
        }
        {
          ipAddress: 'Any'
          action: 'Deny'
          priority: 2147483647
          name: 'Deny all'
          description: 'Deny all access'
        }
      ]

      appSettings: [
        {
          name: 'READ_ONLY_MODE'
          value: (parEnvironment == 'prd') ? 'true' : 'false'
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
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultRef.Name};SecretName=${parApiManagementRef.Name}-${parWebAppName}-repository-subscription-api-key)'
        }
        {
          name: 'portal_servers_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultRef.Name};SecretName=${parApiManagementRef.Name}-${parWebAppName}-servers-integration-subscription-api-key)'
        }
        {
          name: 'geolocation_base_url'
          value: parGeoLocationApi.BaseUrl
        }
        {
          name: 'geolocation_apim_subscription_key_primary'
          value: '@Microsoft.KeyVault(SecretUri=${parGeoLocationApi.KeyVaultPrimaryRef})'
        }
        {
          name: 'geolocation_apim_subscription_key_secondary'
          value: '@Microsoft.KeyVault(SecretUri=${parGeoLocationApi.KeyVaultSecondaryRef})'
        }
        {
          name: 'repository_api_application_audience'
          value: parRepositoryApi.ApplicationAudience
        }
        {
          name: 'servers_api_application_audience'
          value: parServersIntegrationApi.ApplicationAudience
        }
        {
          name: 'geolocation_api_application_audience'
          value: parGeoLocationApi.ApplicationAudience
        }
        {
          name: 'sql_connection_string'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName};Authentication=Active Directory Default; Database=portal-web-${parEnvironmentUniqueId};'
        }
        {
          name: 'xtremeidiots_forums_base_url'
          value: 'https://www.xtremeidiots.com'
        }
        {
          name: 'xtremeidiots_forums_api_key'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultRef.Name};SecretName=xtremeidiots-forums-api-key)'
        }
        {
          name: 'xtremeidiots_auth_client_id'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultRef.Name};SecretName=xtremeidiots-auth-client-id)'
        }
        {
          name: 'xtremeidiots_auth_client_secret'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultRef.Name};SecretName=xtremeidiots-auth-client-secret)'
        }
        {
          name: 'repository_api_path_prefix'
          value: parRepositoryApi.ApimPathPrefix
        }
        {
          name: 'servers_api_path_prefix'
          value: parServersIntegrationApi.ApimPathPrefix
        }
        {
          name: 'geolocation_api_path_prefix'
          value: parGeoLocationApi.ApimPathPrefix
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

// Outputs
output outWebAppDefaultHostName string = webApp.properties.defaultHostName
output outWebAppIdentityPrincipalId string = webApp.identity.principalId
output outWebAppName string = webApp.name
