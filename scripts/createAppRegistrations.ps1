$b3BotsClientName = 'portal-b3bots-client-prd'
az ad app create --display-name $b3BotsClientName
$b3BotsClientId = (az ad app list --filter "displayName eq '$b3BotsClientName'" --query '[].appId') | ConvertFrom-Json

$b3BotsClientSp = az ad sp show --id $b3BotsClientId
if ($null -eq $b3BotsClientSp) {
    az ad sp create --id $b3BotsClientId
}

$eventsApiName = 'portal-events-api-prd'
az ad app create --display-name $eventsApiName --identifier-uris "api://$eventsApiName"
$eventsApiId = (az ad app list --filter "displayName eq '$eventsApiName'" --query '[].appId') | ConvertFrom-Json

$eventsApiSp = az ad sp show --id $eventsApiId
if ($null -eq $eventsApiSp) {
    az ad sp create --id $eventsApiId
}

az ad app update --id $eventsApiId --app-roles @app-registrations/events-api-approles.json

$b3BotsClientPermissions = (az ad app permission list --id $b3botsClientId) | ConvertFrom-Json
$b3BotsClientPermissions

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '85755f6f-35d9-476a-a680-b7e1a12a0e16' }).Count -eq 0) {
    az ad app permission add --id $b3botsClientId --api $eventsApiId --api-permissions '85755f6f-35d9-476a-a680-b7e1a12a0e16=Role' 
}

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '042608f6-317d-43b7-8fad-4618705abfcc' }).Count -eq 0) {
    az ad app permission add --id $b3botsClientId --api $eventsApiId --api-permissions '042608f6-317d-43b7-8fad-4618705abfcc=Role' 
}