param (
    $environment
)

$identity = az functionapp identity show --name fn-repository-portal-$environment-uksouth-01 --resource-group rg-portal-$environment-uksouth-01 | ConvertFrom-Json
$principalId = $identity.principalId

. "./scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalId -environment $environment