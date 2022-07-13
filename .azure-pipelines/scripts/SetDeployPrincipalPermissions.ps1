param (
    $environment,
    $location
)

az keyvault set-policy --name "kv-portal-$environment-$location" --spn $env:servicePrincipalId --secret-permissions get set | Out-Null