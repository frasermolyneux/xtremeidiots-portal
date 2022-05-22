param (
    $environment
)

$identity = az functionapp identity show --name portal-app-leo --resource-group xi-portal-leo | ConvertFrom-Json
$principalId = $identity.principalId

. "./scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalId -environment $environment

. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-identitydb-readers"
. "./scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-portal-$environment-identitydb-writers"