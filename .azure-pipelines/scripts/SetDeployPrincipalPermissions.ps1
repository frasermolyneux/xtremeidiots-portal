param (
    $environment,
    $location
)

az keyvault set-policy --name "kv-portal-$environment-$location" --spn $env:servicePrincipalId --secret-permissions get set | Out-Null

. "./.azure-pipelines/scripts/functions/AddPrincipalToAADGroup.ps1" -principalId $env:servicePrincipalId -groupName "sg-sql-platform-$environment-admins"