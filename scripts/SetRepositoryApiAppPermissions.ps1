param (
    $environment
)

$webApp = (az webapp show --name "webapi-repository-portal-$environment-uksouth-01" --resource-group "rg-portal-$environment-uksouth-01") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-portaldb-readers"
. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-portaldb-writers"