targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parServiceBusName string

param parStorageAccountName string
param parEventsApiAppId string

param parAppServicePlanName string

param parConnectivitySubscriptionId string
param parFrontDoorResourceGroupName string
param parFrontDoorName string

param parWorkloadSubscriptionId string
param parWorkloadResourceGroupName string

param parTags object

// Variables
var varDeploymentPrefix = 'eventsApp'
var varFunctionAppName = 'fn-events-portal-${parEnvironment}-${parLocation}'

// Existing In-Scope Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanName
}

// Existing Out-Of-Scope Resources
resource frontDoor 'Microsoft.Cdn/profiles@2021-06-01' existing = {
  name: parFrontDoorName
  scope: resourceGroup(parConnectivitySubscriptionId, parFrontDoorResourceGroupName)
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: parKeyVaultName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: parAppInsightsName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' existing = {
  name: parStorageAccountName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: parServiceBusName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

// Module Resources
resource functionApp 'Microsoft.Web/sites@2020-06-01' = {
  name: varFunctionAppName
  location: parLocation
  kind: 'functionapp'
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
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
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
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=portal-events-api-${parEnvironment}-clientsecret)'
        }
        {
          name: 'service_bus_connection_string'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${serviceBus.name}-connectionstring)'
        }
      ]
    }
  }
}

resource functionAppStagingSlot 'Microsoft.Web/sites/slots@2020-06-01' = {
  name: 'staging'
  location: parLocation
  kind: 'functionapp'
  parent: functionApp

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
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
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
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=portal-events-api-${parEnvironment}-clientsecret)'
        }
        {
          name: 'service_bus_connection_string'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${serviceBus.name}-connectionstring)'
        }
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

module keyVaultSecret 'br:acrmxplatformprduksouth.azurecr.io/bicep/modules/keyvaultsecret:V2022.07.31.6322' = {
  name: '${varDeploymentPrefix}-keyVaultSecret'
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)

  params: {
    parKeyVaultName: parKeyVaultName
    parSecretName: '${functionApp.name}-hostkey'
    parSecretValue: listkeys('${functionApp.id}/host/default', '2016-08-01').functionKeys.default
    parTags: parTags
  }
}

// Outputs
output outFunctionAppDefaultHostName string = functionApp.properties.defaultHostName
output outFunctionAppIdentityPrincipalId string = functionApp.identity.principalId
output outFunctionAppName string = functionApp.name

output outFunctionAppStagingIdentityPrincipalId string = functionAppStagingSlot.identity.principalId
