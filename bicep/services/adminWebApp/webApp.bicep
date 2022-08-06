targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parAppServicePlanName string
param parSqlServerResourceGroupName string
param parSqlServerName string

param parConnectivitySubscriptionId string
param parFrontDoorResourceGroupName string
param parFrontDoorName string

param parWorkloadSubscriptionId string
param parWorkloadResourceGroupName string

param parTags object

// Variables
var varWebAppName = 'webapp-admin-portal-${parEnvironment}-${parLocation}'

// Existing In-Scope Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanName
}

// Existing Out-Of-Scope Resources
resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' existing = {
  name: parFrontDoorName
  scope: resourceGroup(parConnectivitySubscriptionId, parFrontDoorResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
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
      linuxFxVersion: 'DOTNETCORE|6.0'
      netFrameworkVersion: 'v6.0'
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
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-instrumentationkey)'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-connectionstring)'
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
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varWebAppName}-portal-repository-subscription-apikey)'
        }
        {
          name: 'portal_servers_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varWebAppName}-portal-servers-subscription-apikey)'
        }
        {
          name: 'geolocation_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varWebAppName}-geolocation-subscription-apikey)'
        }
        {
          name: 'repository_api_application_audience'
          value: 'api://portal-repository-api-${parEnvironment}'
        }
        {
          name: 'servers_api_application_audience'
          value: 'api://portal-servers-api-${parEnvironment}'
        }
        {
          name: 'geolocation_api_application_audience'
          value: 'api://geolocation-lookup-api-prd'
        }
        {
          name: 'sql_connection_string'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName};Authentication=Active Directory Default; Database=portalidentitydb-${parEnvironment};'
        }
        {
          name: 'xtremeidiots_forums_base_url'
          value: 'https://www.xtremeidiots.com'
        }
        {
          name: 'xtremeidiots_forums_api_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=xtremeidiots-forums-api-key)'
        }
        {
          name: 'xtremeidiots_auth_client_id'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=xtremeidiots-auth-client-id)'
        }
        {
          name: 'xtremeidiots_auth_client_secret'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=xtremeidiots-auth-client-secret)'
        }
      ]
    }
  }
}

resource webAppStagingSlot 'Microsoft.Web/sites/slots@2020-06-01' = {
  name: 'staging'
  location: parLocation
  kind: 'app'
  parent: webApp

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
      linuxFxVersion: 'DOTNETCORE|6.0'
      netFrameworkVersion: 'v6.0'
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
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-instrumentationkey)'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-connectionstring)'
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
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varWebAppName}-portal-repository-subscription-apikey)'
        }
        {
          name: 'portal_servers_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varWebAppName}-portal-servers-subscription-apikey)'
        }
        {
          name: 'geolocation_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varWebAppName}-geolocation-subscription-apikey)'
        }
        {
          name: 'repository_api_application_audience'
          value: 'api://portal-repository-api-${parEnvironment}'
        }
        {
          name: 'servers_api_application_audience'
          value: 'api://portal-servers-api-${parEnvironment}'
        }
        {
          name: 'geolocation_api_application_audience'
          value: 'api://geolocation-lookup-api-prd'
        }
        {
          name: 'sql_connection_string'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName};Authentication=Active Directory Default; Database=portalidentitydb-${parEnvironment};'
        }
        {
          name: 'xtremeidiots_forums_base_url'
          value: 'https://www.xtremeidiots.com'
        }
        {
          name: 'xtremeidiots_forums_api_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=xtremeidiots-forums-api-key)'
        }
        {
          name: 'xtremeidiots_auth_client_id'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=xtremeidiots-auth-client-id)'
        }
        {
          name: 'xtremeidiots_auth_client_secret'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=xtremeidiots-auth-client-secret)'
        }
      ]
    }
  }
}

// Outputs
output outWebAppDefaultHostName string = webApp.properties.defaultHostName
output outWebAppIdentityPrincipalId string = webApp.identity.principalId
output outWebAppName string = webApp.name

output outWebAppStagingIdentityPrincipalId string = webAppStagingSlot.identity.principalId
