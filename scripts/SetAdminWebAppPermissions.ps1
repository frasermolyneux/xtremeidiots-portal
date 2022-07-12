param (
    $environment
)

$webApp = (az webapp show --name "webapp-admin-portal-$environment-uksouth-01" --resource-group "rg-portal-$environment-uksouth-01") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

$webAppStaging = (az webapp show --name "webapp-admin-portal-$environment-uksouth-01" --resource-group "rg-portal-$environment-uksouth-01" --slot 'staging') | ConvertFrom-Json
$principalIdStaging = $webAppStaging.identity.principalId

. "./scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalId
. "./scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment
. "./scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment
. "./scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalIdStaging

. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-identitydb-readers"
. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-identitydb-writers"
. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-portal-$environment-identitydb-readers"
. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-portal-$environment-identitydb-writers"