param (
    $keyVaultName,
    $applicationName,
    $secretName,
    $secretIdentifier
)

$eventsApiId = (az ad app list --filter "displayName eq '$applicationName'" --query '[].appId') | ConvertFrom-Json
$credentials = (az ad app credential list --id $eventsApiId) | ConvertFrom-Json

if ($credentials.Count -eq 0) {
    $credential = (az ad app credential reset --id $eventsApiId  --append --years 2 --credential-description $secretIdentifier) | ConvertFrom-Json
    az keyvault secret set --name $secretName --vault-name $keyVaultName --value $credential.password --description 'text/plain'
}

if ($credentials.Count -eq 1) {
    $credential = (az ad app credential reset --id $eventsApiId  --append --years 2 --credential-description $secretIdentifier) | ConvertFrom-Json
    az keyvault secret set --name $secretName --vault-name $keyVaultName --value $credential.password --description 'text/plain'
}

if ($credentials.Count -eq 2) {
    $credentialToDelete = $credentials | Sort-Object { Get-Date($_.endDate) } | Select-Object -First 1
    az ad app credential delete --id $eventsApiId --key-id $credentialToDelete.keyId

    $credential = (az ad app credential reset --id $eventsApiId  --append --years 2 --credential-description $secretIdentifier) | ConvertFrom-Json
    az keyvault secret set --name $secretName --vault-name $keyVaultName --value $credential.password --description 'text/plain'
}