$b3BotsClientName = 'portal-b3bots-client-prd'
az ad app create --display-name $b3BotsClientName
$b3BotsClientId = (az ad app list --filter "displayName eq '$b3BotsClientName'" --query '[].appId') | ConvertFrom-Json

$b3BotsClientSp = az ad sp show --id $b3BotsClientId
if ($null -eq $b3BotsClientSp) {
    az ad sp create --id $b3BotsClientId
}

$eventsApiAppName = 'portal-events-api-prd'
az ad app create --display-name $eventsApiAppName --identifier-uris "api://$eventsApiAppName"
$eventsApiAppId = (az ad app list --filter "displayName eq '$eventsApiAppName'" --query '[].appId') | ConvertFrom-Json

$eventsApiAppSp = az ad sp show --id $eventsApiAppId
if ($null -eq $eventsApiAppSp) {
    az ad sp create --id $eventsApiAppId
}

az ad app update --id $eventsApiAppId --app-roles @app-registrations/events-api-approles.json

$b3BotsClientPermissions = (az ad app permission list --id $b3botsClientId) | ConvertFrom-Json
$b3BotsClientPermissions

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '85755f6f-35d9-476a-a680-b7e1a12a0e16' }).Count -eq 0) {
    az ad app permission add --id $b3botsClientId --api $eventsApiAppId --api-permissions '85755f6f-35d9-476a-a680-b7e1a12a0e16=Role' 
}

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '042608f6-317d-43b7-8fad-4618705abfcc' }).Count -eq 0) {
    az ad app permission add --id $b3botsClientId --api $eventsApiAppId --api-permissions '042608f6-317d-43b7-8fad-4618705abfcc=Role' 
}

## Repository API
$repositoryApiName = 'portal-repository-api-prd'
az ad app create --display-name $repositoryApiName --identifier-uris "api://$repositoryApiName"
$repositoryApiId = (az ad app list --filter "displayName eq '$repositoryApiName'" --query '[].appId') | ConvertFrom-Json

$repositoryApiSp = az ad sp show --id $repositoryApiId
if ($null -eq $repositoryApiSp) {
    az ad sp create --id $repositoryApiId
}

az ad app update --id $repositoryApiId --app-roles @app-registrations/repository-api-approles.json

## Servers API
$serversApiName = 'portal-servers-api-prd'
az ad app create --display-name $serversApiName --identifier-uris "api://$serversApiName"
$serversApiId = (az ad app list --filter "displayName eq '$serversApiName'" --query '[].appId') | ConvertFrom-Json

$serversApiSp = az ad sp show --id $serversApiId
if ($null -eq $serversApiSp) {
    az ad sp create --id $serversApiId
}

az ad app update --id $serversApiId --app-roles @app-registrations/servers-api-approles.json