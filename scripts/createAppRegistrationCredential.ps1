param (
    $keyVaultName,
    $applicationName,
    $secretName,
    $secretIdentifier
)

$applicationId = (az ad app list --filter "displayName eq '$applicationName'" --query '[].appId') | ConvertFrom-Json
$credentials = (az ad app credential list --id $applicationId) | ConvertFrom-Json

if ($credentials.Count -eq 0) {
    $credential = (az ad app credential reset --id $applicationId  --append --years 2 --display-name $secretIdentifier) | ConvertFrom-Json
    az keyvault secret set --name $secretName --vault-name $keyVaultName --value $credential.password --description 'text/plain' | out-null
}

if ($credentials.Count -eq 1) {
    $credential = (az ad app credential reset --id $applicationId  --append --years 2 --display-name $secretIdentifier) | ConvertFrom-Json
    az keyvault secret set --name $secretName --vault-name $keyVaultName --value $credential.password --description 'text/plain' | out-null
}

if ($credentials.Count -eq 2) {
    $credentialToDelete = $credentials | Sort-Object { Get-Date($_.endDateTime) } | Select-Object -First 1
    az ad app credential delete --id $applicationId --key-id $credentialToDelete.keyId

    $credential = (az ad app credential reset --id $applicationId  --append --years 2 --display-name $secretIdentifier) | ConvertFrom-Json
    az keyvault secret set --name $secretName --vault-name $keyVaultName --value $credential.password --description 'text/plain' | out-null
}