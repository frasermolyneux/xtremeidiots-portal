targetScope = 'resourceGroup'

// Parameters
@description('The environment name (e.g. dev, tst, prd).')
param parEnvironment string

@description('The environment unique id (e.g. 1234).')
param parEnvironmentUniqueId string

@description('The location of the resource group.')
param parLocation string

@description('The instance name (e.g. 01, 02, 03).')
param parInstance string

@description('The name of the key vault.')
param parKeyVaultName string

@description('The name of the application insights.')
param parAppInsightsName string

@description('The repository api object.')
param parRepositoryApi object

@description('The servers integration api object.')
param parServersIntegrationApi object

@description('The geo location api object.')
param parGeoLocationApi object

@description('The strategic services subscription id.')
param parStrategicServicesSubscriptionId string

@description('The api management resource group name.')
param parApiManagementResourceGroupName string

@description('The api management name.')
param parApiManagementName string

@description('The app service plan name.')
param parAppServicePlanName string

@description('The sql server resource group name.')
param parSqlServerResourceGroupName string

@description('The sql server name.')
param parSqlServerName string

@description('The front door subscription id.')
param parFrontDoorSubscriptionId string

@description('The front door resource group name.')
param parFrontDoorResourceGroupName string

@description('The front door name.')
param parFrontDoorName string

@description('The tags to apply to the resources.')
param parTags object

// Variables
var varWebAppName = 'app-portal-web-${parEnvironment}-${parLocation}-${parInstance}-${parEnvironmentUniqueId}'

// Existing In-Scope Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanName
}

// Existing Out-Of-Scope Resources
resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' existing = {
  name: parFrontDoorName
  scope: resourceGroup(parFrontDoorSubscriptionId, parFrontDoorResourceGroupName)
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)
}

resource sqlServer 'Microsoft.Sql/servers@2021-11-01-preview' existing = {
  name: parSqlServerName
  scope: resourceGroup(parStrategicServicesSubscriptionId, parSqlServerResourceGroupName)
}

// Module Resources
resource webApp 'Microsoft.Web/sites@2020-06-01' = {
  name: varWebAppName
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
      linuxFxVersion: 'DOTNETCORE|7.0'
      netFrameworkVersion: 'v7.0'
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
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=${parAppInsightsName}-instrumentationkey)'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=${parAppInsightsName}-connectionstring)'
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
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=${parApiManagementName}-${varWebAppName}-repository-subscription-apikey)'
        }
        {
          name: 'portal_servers_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=${parApiManagementName}-${varWebAppName}-servers-integration-subscription-apikey)'
        }
        {
          name: 'geolocation_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=${parApiManagementName}-${varWebAppName}-geolocation-subscription-apikey)'
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
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=xtremeidiots-forums-api-key)'
        }
        {
          name: 'xtremeidiots_auth_client_id'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=xtremeidiots-auth-client-id)'
        }
        {
          name: 'xtremeidiots_auth_client_secret'
          value: '@Microsoft.KeyVault(VaultName=${parKeyVaultName};SecretName=xtremeidiots-auth-client-secret)'
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
      ]
    }
  }
}

// Outputs
output outWebAppDefaultHostName string = webApp.properties.defaultHostName
output outWebAppIdentityPrincipalId string = webApp.identity.principalId
output outWebAppName string = webApp.name
