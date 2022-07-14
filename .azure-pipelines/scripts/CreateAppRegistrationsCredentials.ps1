param (
    $environment,
    $location
)

. "./.azure-pipelines/scripts/functions/CreateAppRegistrationCredential.ps1" `
    -keyVaultName "kv-portal-$environment-$location" `
    -applicationName "portal-events-api-$environment" `
    -secretPrefix "portal-events-api-$environment" `
    -secretDisplayName "fnportalevents"

. "./.azure-pipelines/scripts/functions/CreateAppRegistrationCredential.ps1" `
    -keyVaultName "kv-portal-$environment-$location" `
    -applicationName "portal-repository-api-$environment" `
    -secretPrefix "portal-repository-api-$environment" `
    -secretDisplayName "webportalrepo"

. "./.azure-pipelines/scripts/functions/CreateAppRegistrationCredential.ps1" `
    -keyVaultName "kv-portal-$environment-$location" `
    -applicationName "portal-servers-api-$environment" `
    -secretPrefix "portal-servers-api-$environment" `
    -secretDisplayName "webportalsvrs"