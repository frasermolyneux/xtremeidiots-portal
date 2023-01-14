param (
    $environment
)

$webApp = (az webapp show --name "webapp-admin-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth") | ConvertFrom-Json
$principalId = $webApp.identity.principalId

Write-Host "Web App 'webapp-admin-portal-$environment-uksouth' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalId'"

. "./.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalId

. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalId -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-writers"

$webAppStaging = (az webapp show --name "webapp-admin-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" --slot 'staging') | ConvertFrom-Json
$principalIdStaging = $webAppStaging.identity.principalId

Write-Host "Web App Slot 'webapp-admin-portal-$environment-uksouth/staging' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalIdStaging'"

. "./.azure-pipelines/scripts/functions/GrantLookupApiPermissionsToApp.ps1" -principalId $principalIdStaging

. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-readers"
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $principalIdStaging -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-writers"

# Grant permissions to Repository API
$repositoryApiId = (az ad app list --filter "displayName eq 'portal-repository-$environment'" --query '[].appId') | ConvertFrom-Json
$repositoryApiSpnId = (az ad sp list --filter "appId eq '$repositoryApiId'" --query '[0].id') | ConvertFrom-Json
$repositoryApiSpn = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$resourceId) | ConvertFrom-Json
$repositoryAppRoleId = ($repositoryApiSpn.appRoles | Where-Object { $_.displayName -eq "ServiceAccount" }).id

. "./.azure-pipelines/scripts/functions/GrantPrincipalAppRole.ps1" -principalId "$($webApp.identity.principalId)" -resourceId $repositoryApiSpnId -appRoleId $repositoryAppRoleId
. "./.azure-pipelines/scripts/functions/GrantPrincipalAppRole.ps1" -principalId "$($webAppStaging.identity.principalId)" -resourceId $repositoryApiSpnId -appRoleId $repositoryAppRoleId

# Grant permissions to Servers API
$serversApiId = (az ad app list --filter "displayName eq 'portal-servers-$environment'" --query '[].appId') | ConvertFrom-Json
$serversApiSpnId = (az ad sp list --filter "appId eq '$serversApiId'" --query '[0].id') | ConvertFrom-Json
$serversApiSpn = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$resourceId) | ConvertFrom-Json
$serversAppRoleId = ($serversApiSpn.appRoles | Where-Object { $_.displayName -eq "ServiceAccount" }).id

. "./.azure-pipelines/scripts/functions/GrantPrincipalAppRole.ps1" -principalId "$($webApp.identity.principalId)" -resourceId $serversApiSpnId -appRoleId $serversAppRoleId
. "./.azure-pipelines/scripts/functions/GrantPrincipalAppRole.ps1" -principalId "$($webAppStaging.identity.principalId)" -resourceId $serversApiSpnId -appRoleId $serversAppRoleId