param (
    $environment,
    $location
)

. "./.azure-pipelines/scripts/functions/CreateAppRegistrationCredential.ps1" `
    -keyVaultName "kv-portal-$environment-$location" `
    -applicationName "portal-events-api-$environment" `
    -secretPrefix "portal-events-api-$environment" `
    -secretDisplayName "fnportalevents"
