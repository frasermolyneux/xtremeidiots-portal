$environment = 'prd'
$location = 'uksouth'

$keyVaultName = "kv-portal-$environment-$location-01"
$secretName = "sql-portal-$environment-$location-01-admin-password"

$password =  ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!£$%^&*()".tochararray() | sort {Get-Random})[0..18] -join ''

az keyvault secret set --name $secretName --vault-name $keyVaultName --value $password --description 'text/plain'