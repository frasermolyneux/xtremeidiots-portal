param (
    $environment
)

$webApp = (az webapp show --name "webapp-admin-portal-$environment-uksouth-01" --resource-group "rg-portal-$environment-uksouth-01") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

. "./scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalId -environment $environment

. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-identitydb-readers"
. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-identitydb-writers"