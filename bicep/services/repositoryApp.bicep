targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppServicePlanName string
param parAppInsightsName string
param parApiManagementName string
param parServiceBusName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parPlatformApiManagementName string
param parTags object

// Variables
var varRepositoryFuncAppName = 'fn-repository-portal-${parEnvironment}-${parLocation}-01'

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

resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: parServiceBusName
}

resource platformApiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parPlatformApiManagementName
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)
}

// Module Resources
module adminWebAppGeoLocationApiManagementSubscription './../modules/apiManagementSubscription.bicep' = {
  name: 'publicWebAppApiManagementSubscription'
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)

  params: {
    parApiManagementName: platformApiManagement.name
    parWorkloadSubscriptionId: subscription().subscriptionId
    parWorkloadResourceGroupName: resourceGroup().name
    parWorkloadName: varRepositoryFuncAppName
    parKeyVaultName: keyVault.name
    parSubscriptionScope: '/apis/geolocation-api'
    parTags: parTags
  }
}

resource apiManagementSubscription 'Microsoft.ApiManagement/service/subscriptions@2021-08-01' = {
  name: '${apiManagement.name}-${varRepositoryFuncAppName}-subscription'
  parent: apiManagement

  properties: {
    allowTracing: false
    displayName: varRepositoryFuncAppName
    scope: '/apis'
  }
}

resource functionHostKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: '${apiManagement.name}-${varRepositoryFuncAppName}-apikey'
  parent: keyVault

  properties: {
    contentType: 'text/plain'
    value: apiManagementSubscription.properties.primaryKey
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: 'sa${uniqueString(resourceGroup().id)}${parEnvironment}'
  location: parLocation
  kind: 'StorageV2'

  sku: {
    name: 'Standard_LRS'
  }
}

resource functionApp 'Microsoft.Web/sites@2020-06-01' = {
  name: varRepositoryFuncAppName
  location: parLocation
  kind: 'functionapp'

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: appServicePlan.id

    httpsOnly: true

    siteConfig: {
      alwaysOn: true
      ftpsState: 'Disabled'

      appSettings: [
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
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${appInsights.name}-connectionstring)'
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
          name: 'apim-base-url'
          value: apiManagement.properties.gatewayUrl
        }
        {
          name: 'apim-subscription-key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varRepositoryFuncAppName}-apikey)'
        }
        {
          name: 'repository-api-application-audience'
          value: 'api://portal-repository-api-${parEnvironment}'
        }
        {
          name: 'servers-api-application-audience'
          value: 'api://portal-servers-api-${parEnvironment}'
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
          name: 'geolocation_apim_base_url'
          value: 'https://apim-mx-platform-prd-uksouth.azure-api.net'
        }
        {
          name: 'geolocation_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${platformApiManagement.name}-${varRepositoryFuncAppName}-subscription-apikey)'
        }
        {
          name: 'geolocation_api_application_audience'
          value: 'api://geolocation-lookup-api-prd'
        }
      ]
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
