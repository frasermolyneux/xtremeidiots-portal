param (
    $environment
)

$identity = az functionapp identity show --name "webapi-servers-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" | ConvertFrom-Json
$principalId = $identity.principalId

Write-Host "Web App 'webapi-servers-portal-$environment-uksouth' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalId'"

. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalId -environment $environment

$identityStaging = az functionapp identity show --name "webapi-servers-portal-$environment-uksouth" --resource-group "rg-platform-webapps-prd-uksouth" --slot 'staging' | ConvertFrom-Json
$principalIdStaging = $identityStaging.principalId

Write-Host "Web App Slot 'webapi-servers-portal-$environment-uksouth/staging' in resource group 'rg-platform-webapps-prd-uksouth' has principal id '$principalIdStaging'"

. "./.azure-pipelines/scripts/functions/GrantRepositoryApiPermissionsToApp.ps1" -principalId $principalIdStaging -environment $environment