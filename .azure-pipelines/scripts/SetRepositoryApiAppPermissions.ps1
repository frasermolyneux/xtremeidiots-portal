param (
    $environment
)

$webApp = (az webapp show --name "webapi-repository-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

Write-Host "Web App 'webapi-repository-portal-$environment-uksouth' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalId'"

. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portaldb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portaldb-$environment-writers"

$webAppStaging = (az webapp show --name "webapi-repository-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" --slot 'staging') | ConvertFrom-Json
$principalIdStaging = $webAppStaging.identity.principalId

Write-Host "Web App Slot 'webapi-repository-portal-$environment-uksouth/staging' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalIdStaging'"

. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portaldb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portaldb-$environment-writers"