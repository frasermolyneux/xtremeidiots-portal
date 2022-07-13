param (
    $keyVaultName,
    $applicationName,
    $secretPrefix,
    $secretDisplayName
)

$applicationId = (az ad app list --filter "displayName eq '$applicationName'" --query '[].appId') | ConvertFrom-Json
$credentials = (az ad app credential list --id $applicationId) | ConvertFrom-Json

az keyvault secret set --name "$secretPrefix-clientid" --vault-name $keyVaultName --value $applicationId --description 'text/plain' | Out-Null

if ($credentials.Count -eq 0) {
    $credential = (az ad app credential reset --id $applicationId  --append --years 2 --display-name $secretDisplayName) | ConvertFrom-Json
    az keyvault secret set --name "$secretPrefix-clientsecret" --vault-name $keyVaultName --value $credential.password --description 'text/plain' | Out-Null
}

if ($credentials.Count -eq 1) {
    $credential = (az ad app credential reset --id $applicationId  --append --years 2 --display-name $secretDisplayName) | ConvertFrom-Json
    az keyvault secret set --name "$secretPrefix-clientsecret" --vault-name $keyVaultName --value $credential.password --description 'text/plain' | Out-Null
}

if ($credentials.Count -eq 2) {
    $credentialToDelete = $credentials | Sort-Object { Get-Date($_.endDateTime) } | Select-Object -First 1
    az ad app credential delete --id $applicationId --key-id $credentialToDelete.keyId

    $credential = (az ad app credential reset --id $applicationId  --append --years 2 --display-name $secretDisplayName) | ConvertFrom-Json
    az keyvault secret set --name "$secretPrefix-clientsecret" --vault-name $keyVaultName --value $credential.password --description 'text/plain' | Out-Null
}