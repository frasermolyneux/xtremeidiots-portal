param (
    $environment,
    $location
)

. "./.azure-pipelines/scripts/functions/CreateAppRegistration.ps1" `
    -applicationName "portal-b3bots-client-$environment"

. "./.azure-pipelines/scripts/functions/CreateAppRegistration.ps1" `
    -applicationName "portal-events-api-$environment" `
    -appRoles "events-api-approles.json"

. "./.azure-pipelines/scripts/functions/CreateAppRegistration.ps1" `
    -applicationName "portal-repository-api-$environment" `
    -appRoles "repository-api-approles.json"

. "./.azure-pipelines/scripts/functions/CreateAppRegistration.ps1" `
    -applicationName "portal-servers-api-$environment" `
    -appRoles "servers-api-approles.json"

# TODO: Sort this next bit of code out
$eventsApiAppId = (az ad app list --filter "displayName eq 'portal-events-api-$environment'" --query '[].appId') | ConvertFrom-Json
$b3BotsClientId = (az ad app list --filter "displayName eq 'portal-b3bots-client-$environment'" --query '[].appId') | ConvertFrom-Json
$b3BotsClientPermissions = (az ad app permission list --id "$b3botsClientId") | ConvertFrom-Json

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '85755f6f-35d9-476a-a680-b7e1a12a0e16' }).Count -eq 0) {
    az ad app permission add --id "$b3botsClientId" --api "$eventsApiAppId" --api-permissions '85755f6f-35d9-476a-a680-b7e1a12a0e16=Role' 
}

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '042608f6-317d-43b7-8fad-4618705abfcc' }).Count -eq 0) {
    az ad app permission add --id "$b3botsClientId" --api "$eventsApiAppId" --api-permissions '042608f6-317d-43b7-8fad-4618705abfcc=Role' 
}

. "./.azure-pipelines/scripts/functions/CreateAppRegistrationCredential.ps1" `
    -keyVaultName "kv-portal-$environment-$location" `
    -applicationName "portal-events-api-$location" `
    -secretPrefix "portal-events-api-$location" `
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