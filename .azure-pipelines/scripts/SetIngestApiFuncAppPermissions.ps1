param (
    $environment
)

$identity = az functionapp identity show --name "fn-ingest-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" | ConvertFrom-Json
$principalId = $identity.principalId

Write-Host "Function App 'fn-ingest-portal-$environment-uksouth' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalId'"

. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment