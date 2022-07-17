param (
    $environment
)

$webApp = (az webapp show --name "webapp-admin-portal-$environment-uksouth" --resource-group "rg-portal-$environment-uksouth") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

$webAppStaging = (az webapp show --name "webapp-admin-portal-$environment-uksouth" --resource-group "rg-portal-$environment-uksouth" --slot 'staging') | ConvertFrom-Json
$principalIdStaging = $webAppStaging.identity.principalId

. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./.azure-pipelines/scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "./.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalId
. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment
. "./.azure-pipelines/scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment
. "./.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalIdStaging

. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-writers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-writers"