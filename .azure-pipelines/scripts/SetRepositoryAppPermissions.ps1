param (
    $environment
)

$identity = az functionapp identity show --name fn-repository-portal-$environment-uksouth-01 --resource-group rg-portal-$environment-uksouth-01 | ConvertFrom-Json
$principalId = $identity.principalId

. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./.azure-pipelines/scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalId