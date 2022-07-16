targetScope = 'resourceGroup'

// Parameters
param parLocation string
param parEnvironment string
param parKeyVaultName string
param parAppInsightsName string
param parServiceBusName string

param parStorageAccountName string

param parStrategicServicesSubscriptionId string
param parApiManagementResourceGroupName string
param parApiManagementName string
param parAppServicePlanName string

param parWorkloadSubscriptionId string
param parWorkloadResourceGroupName string

param parTags object

// Variables
var varFunctionAppName = 'fn-ingest-portal-${parEnvironment}-${parLocation}'

// Existing In-Scope Resources
resource appServicePlan 'Microsoft.Web/serverfarms@2020-10-01' existing = {
  name: parAppServicePlanName
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

resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' existing = {
  name: parStorageAccountName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
  name: parServiceBusName
  scope: resourceGroup(parWorkloadSubscriptionId, parWorkloadResourceGroupName)
}

resource apiManagement 'Microsoft.ApiManagement/service@2021-12-01-preview' existing = {
  name: parApiManagementName
  scope: resourceGroup(parStrategicServicesSubscriptionId, parApiManagementResourceGroupName)
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
          name: 'service_bus_connection_string'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${serviceBus.name}-connectionstring)'
        }
        {
          name: 'apim_base_url'
          value: apiManagement.properties.gatewayUrl
        }
        {
          name: 'portal_repository_apim_subscription_key'
          value: '@Microsoft.KeyVault(VaultName=${keyVault.name};SecretName=${apiManagement.name}-${varFunctionAppName}-portal-repository-subscription-apikey)'
        }
        {
          name: 'repository_api_application_audience'
          value: 'api://portal-repository-api-${parEnvironment}'
        }
      ]
    }
  }
}

module functionAppHostKeySecret './../../modules/keyVaultSecret.bicep' = {
  name: 'functionAppHostKeySecret'
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
