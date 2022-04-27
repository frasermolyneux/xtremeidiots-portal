#az login
#az account set --subscription 32444f38-32f4-409f-889c-8e8aa2b5b4d1
#az account show

# Top-level Params
$subscription = '32444f38-32f4-409f-889c-8e8aa2b5b4d1'
$location = 'uksouth'
$environment = 'prd'

# Deploy core resources
az deployment sub create --subscription $subscription `
    --template-file bicep/portalCore.bicep  `
    --location $location `
    --parameters parLocation=$location parEnvironment=$environment

# Core Resource Names
$resourceGroup = "rg-portal-$environment-$location-01"
$keyVault = "kv-portal-$environment-$location-01"
$functionServicePlan = "plan-fn-portal-$environment-$location-01"
$appInsights = "ai-portal-$environment-$location-01"
$apiManagement = "apim-portal-$environment-$location-01"
$serviceBus = "sb-portal-$environment-$location-01"
$webServicePlan = "plan-web-portal-$environment-$location-01"

# Application Names
$eventsApiAppName = "portal-events-api-$environment"
$repositoryApiName = "portal-repository-api-$environment"

## Application IDs
$eventsApiAppId = (az ad app list --filter "displayName eq '$eventsApiAppName'" --query '[].appId') | ConvertFrom-Json
$repositoryApiAppId = (az ad app list --filter "displayName eq '$repositoryApiName'" --query '[].appId') | ConvertFrom-Json

# Deploy Apps
az deployment group create --resource-group $resourceGroup `
    --template-file bicep/repositoryApp.bicep `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parFuncAppServicePlanName=$functionServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/repositoryApi.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parWebAppServicePlanName=$webServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parRepositoryApiAppId=$repositoryApiAppId

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/ingestApp.bicep `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parFuncAppServicePlanName=$functionServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/eventsApp.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parFuncAppServicePlanName=$functionServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus `
    parEventsApiAppId=$eventsApiAppId

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/syncApp.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parFuncAppServicePlanName=$functionServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus
