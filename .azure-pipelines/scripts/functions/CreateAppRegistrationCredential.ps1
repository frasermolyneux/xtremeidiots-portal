param (
    $keyVaultName,
    $applicationName,
    $secretPrefix,
    $secretDisplayName
)

$applicationId = (az ad app list --filter "displayName eq '$applicationName'" --query '[].appId') | ConvertFrom-Json
$credentials = (az ad app credential list --id "$applicationId") | ConvertFrom-Json

if ($null -eq (az keyvault secret show --vault-name "$keyVaultName" --name "$secretPrefix-clientid")) {
    Write-Host "Creating client id secret with name '$secretPrefix-clientid' in '$keyVaultName'"
    az keyvault secret set --name "$secretPrefix-clientid" --vault-name "$keyVaultName" --value "$applicationId" --description 'text/plain' | Out-Null
}

if ($credentials.Count -eq 0) {
    Write-Host "No credentials: Creating credential and storing in Key Vault with name '$secretPrefix-clientsecret' in '$keyVaultName'"
    $credential = (az ad app credential reset --id "$applicationId"  --append --years 2 --display-name "$secretDisplayName") | ConvertFrom-Json
    az keyvault secret set --name "$secretPrefix-clientsecret" --vault-name "$keyVaultName" --value $credential.password --description 'text/plain' | Out-Null
}

if ($credentials.Count -eq 1) {
    Write-Host "Single credential: Creating credential and storing in Key Vault with name '$secretPrefix-clientsecret' in '$keyVaultName'"
    $credential = (az ad app credential reset --id "$applicationId"  --append --years 1 --display-name "$secretDisplayName") | ConvertFrom-Json
    az keyvault secret set --name "$secretPrefix-clientsecret" --vault-name "$keyVaultName" --value $credential.password --description 'text/plain' | Out-Null
}

if ($credentials.Count -eq 2) {
    Write-Host "Multiple credential: Checking to see if we need to rotate the secret"

    $credentials | ForEach-Object {
        Write-Host "Credential expires in '$($_.endDateTime)'"
    }

    $credentialToDelete = $credentials | Sort-Object { Get-Date($_.endDateTime) } | Select-Object -First 1
    $expiryDate = Get-Date($credentialToDelete.endDateTime)

    if ($expiryDate -lt (Get-Date).AddMonths(3)) {
        Write-Host "Near Expiry Credential: Reset credential with expiry '$($credentialToDelete.endDateTime)' and store in Key Vault with name '$secretPrefix-clientsecret' in '$keyVaultName'"

        az ad app credential delete --id "$applicationId" --key-id $credentialToDelete.keyId

        $credential = (az ad app credential reset --id "$applicationId"  --append --years 1 --display-name $secretDisplayName) | ConvertFrom-Json
        az keyvault secret set --name "$secretPrefix-clientsecret" --vault-name "$keyVaultName" --value $credential.password --description 'text/plain' | Out-Null
    }
    else {
        Write-Host "No credentials are near expiry - doing nothing"
    }
}