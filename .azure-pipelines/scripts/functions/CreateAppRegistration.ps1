param (
    $applicationName,
    $appRoles = $null
)

az ad app create --display-name "$applicationName" --identifier-uris "api://$applicationName" | Out-Null
$applicationId = (az ad app list --filter "displayName eq '$applicationName'" --query '[].appId') | ConvertFrom-Json

az ad app update --id "$applicationId" --sign-in-audience 'AzureADMyOrg' --enable-id-token-issuance true --enable-access-token-issuance false | Out-Null

$applicationServicePrincipal = az ad sp show --id "$applicationId"
if ($null -eq $applicationServicePrincipal) {
    az ad sp create --id "$applicationId" | Out-Null
}

if ($null -ne $appRoles) {
    az ad app update --id "$applicationId" --app-roles ".azure-pipelines/app-registrations/$appRoles" | Out-Null
}
