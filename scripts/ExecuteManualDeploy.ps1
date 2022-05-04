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
$appServicePlan = "plan-portal-$environment-$location-01"
$appInsights = "ai-portal-$environment-$location-01"
$apiManagement = "apim-portal-$environment-$location-01"
$serviceBus = "sb-portal-$environment-$location-01"
$sqlServerName = "sql-portal-$environment-$location-01"

## Secret Names
$sqlPasswordSecretName = "sql-portal-$environment-$location-01-admin-password"

# Application Names
$eventsApiAppName = "portal-events-api-$environment"
$repositoryApiName = "portal-repository-api-$environment"

## Application IDs
$eventsApiAppId = (az ad app list --filter "displayName eq '$eventsApiAppName'" --query '[].appId') | ConvertFrom-Json
$repositoryApiAppId = (az ad app list --filter "displayName eq '$repositoryApiName'" --query '[].appId') | ConvertFrom-Json

## AAD Groups Names
$sqlAdminGroupName = "sg-sql-portal-$environment-admins"

## AAD Group IDs 
$adminGroupObjectId = (az ad group show --group $sqlAdminGroupName --query 'objectId')  | ConvertFrom-Json

# Deploy Database
$sqlServerAdminPassword = (az keyvault secret show --name $sqlPasswordSecretName --vault-name $keyVault --query 'value') | ConvertFrom-Json

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/sqlServer.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parAdminPassword=$sqlServerAdminPassword `
    parKeyVaultName=$keyVault `
    parAdminGroupName=$adminGroupName `
    parAdminGroupOid=$adminGroupObjectId

# Deploy Apps
az deployment group create --resource-group $resourceGroup `
    --template-file bicep/repositoryApp.bicep `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parAppServicePlanName=$appServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/repositoryApi.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parAppServicePlanName=$appServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parSqlServerName=$sqlServerName `
    parRepositoryApiAppId=$repositoryApiAppId

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/ingestApp.bicep `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parAppServicePlanName=$appServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/eventsApp.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parAppServicePlanName=$appServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus `
    parEventsApiAppId=$eventsApiAppId

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/syncApp.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parAppServicePlanName=$appServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus

az deployment group create --resource-group $resourceGroup `
    --template-file bicep/adminWebApp.bicep  `
    --parameters parLocation=$location `
    parEnvironment=$environment `
    parKeyVaultName=$keyVault `
    parAppServicePlanName=$appServicePlan `
    parAppInsightsName=$appInsights `
    parApiManagementName=$apiManagement `
    parServiceBusName=$serviceBus