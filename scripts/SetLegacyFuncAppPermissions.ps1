param (
    $environment
)

$identity = az functionapp identity show --name portal-funcapp-leo --resource-group xi-portal-leo | ConvertFrom-Json
$principalId = $identity.principalId

. "./scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment