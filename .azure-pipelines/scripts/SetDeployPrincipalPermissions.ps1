param (
    $environment,
    $location
)

az keyvault set-policy --name "kv-portal-$environment-$location" --spn $env:servicePrincipalId --secret-permissions get set | Out-Null

$spn = (az ad sp show --id $env:servicePrincipalId) | ConvertFrom-Json
. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $spn.id -groupName "sg-sql-platform-$environment-admins"