param (
    $environment
)

$identity = az functionapp identity show --name webapi-servers-portal-$environment-uksouth-01 --resource-group rg-portal-$environment-uksouth-01 | ConvertFrom-Json
$principalId = $identity.principalId

. "./scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment