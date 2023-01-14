param (
    $environment
)

$identity = az functionapp identity show --name "fn-sync-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" | ConvertFrom-Json
$principalId = $identity.principalId

Write-Host "Function App 'fn-sync-portal-$environment-uksouth' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalId'"

# Grant permissions to Repository API
$repositoryApiId = (az ad app list --filter "displayName eq 'portal-repository-$environment'" --query '[].appId') | ConvertFrom-Json
$repositoryApiSpnId = (az ad sp list --filter "appId eq '$repositoryApiId'" --query '[0].id') | ConvertFrom-Json
$repositoryApiSpn = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$resourceId) | ConvertFrom-Json
$appRoleId = ($repositoryApiSpn.appRoles | Where-Object { $_.displayName -eq "ServiceAccount" }).id

. "./.azure-pipelines/scripts/functions/GrantPrincipalAppRole.ps1" -principalId $identity.principalId -resourceId $repositoryApiSpnId -appRoleId $appRoleId