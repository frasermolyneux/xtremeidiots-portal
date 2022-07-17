param (
    $environment
)

$webApp = (az webapp show --name "webapi-repository-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

$webAppStaging = (az webapp show --name "webapi-repository-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" --slot 'staging') | ConvertFrom-Json
$principalIdStaging = $webAppStaging.identity.principalId


. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portaldb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portaldb-$environment-writers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portaldb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portaldb-$environment-writers"