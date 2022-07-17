param (
    $environment
)

$webApp = (az webapp show --name "webapp-admin-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

Write-Host "Web App 'webapp-admin-portal-$environment-uksouth' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalId'"

. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalId -environment $environment
. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalId

. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-readers"
. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-writers"

$webAppStaging = (az webapp show --name "webapp-admin-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" --slot 'staging') | ConvertFrom-Json
$principalIdStaging = $webAppStaging.identity.principalId

Write-Host "Web App Slot 'webapp-admin-portal-$environment-uksouth/staging' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalIdStaging'"

. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment
. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/GrantServersApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment
. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalIdStaging

. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-readers"
. "c:\Git\frasermolyneux\xtremeidiots-portal/.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-writers"