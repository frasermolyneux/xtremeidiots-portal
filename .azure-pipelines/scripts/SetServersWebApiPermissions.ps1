param (
    $environment
)

$identity = az functionapp identity show --name "webapi-servers-portal-$environment-uksouth" --resource-group "rg-portal-$environment-uksouth" | ConvertFrom-Json
$principalId = $identity.principalId

$identityStaging = az functionapp identity show --name "webapi-servers-portal-$environment-uksouth" --resource-group "rg-portal-$environment-uksouth" --slot 'staging' | ConvertFrom-Json
$principalIdStaging = $identityStaging.principalId

. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment