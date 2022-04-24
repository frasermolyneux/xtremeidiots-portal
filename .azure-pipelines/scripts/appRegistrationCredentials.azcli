$keyVaultName = 'kv-portal-prd-uksouth-01'
$eventsApiName = 'portal-events-api-prd'

$eventsApiId = (az ad app list --filter "displayName eq '$eventsApiName'" --query '[].appId') | ConvertFrom-Json
$credentials = (az ad app credential list --id $eventsApiId) | ConvertFrom-Json

if ($credentials.Count -eq 0) {
    $credential = (az ad app credential reset --id $eventsApiId  --append --years 2 --credential-description 'fnportalevents') | ConvertFrom-Json
    az keyvault secret set --name 'portal-events-api-prd-clientsecret' --vault-name $keyVaultName --value $credential.password --description 'text/plain'
}

if ($credentials.Count -eq 1) {
    $credential = (az ad app credential reset --id $eventsApiId  --append --years 2 --credential-description 'fnportalevents') | ConvertFrom-Json
    az keyvault secret set --name 'portal-events-api-prd-clientsecret' --vault-name $keyVaultName --value $credential.password --description 'text/plain'
}

if ($credentials.Count -eq 2) {
    $credentialToDelete = $credentials | Sort-Object {Get-Date($_.endDate)} | Select -First 1
    az ad app credential delete --id $eventsApiId --key-id $credentialToDelete.keyId

    $credential = (az ad app credential reset --id $eventsApiId  --append --years 2 --credential-description 'fnportalevents') | ConvertFrom-Json
    az keyvault secret set --name 'portal-events-api-prd-clientsecret' --vault-name $keyVaultName --value $credential.password --description 'text/plain'
}