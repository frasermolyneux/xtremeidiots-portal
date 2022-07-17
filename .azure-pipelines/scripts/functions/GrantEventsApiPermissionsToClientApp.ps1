$b3BotsClientId = (az ad app list --filter "displayName eq '$b3BotsClientName'" --query '[].appId') | ConvertFrom-Json

$b3BotsClientPermissions = (az ad app permission list --id "$b3botsClientId") | ConvertFrom-Json
$b3BotsClientPermissions

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '85755f6f-35d9-476a-a680-b7e1a12a0e16' }).Count -eq 0) {
    az ad app permission add --id "$b3botsClientId" --api "$eventsApiAppId" --api-permissions '85755f6f-35d9-476a-a680-b7e1a12a0e16=Role' 
}

if (($b3BotsClientPermissions.resourceAccess | Where-Object { $_.id -eq '042608f6-317d-43b7-8fad-4618705abfcc' }).Count -eq 0) {
    az ad app permission add --id "$b3botsClientId" --api "$eventsApiAppId" --api-permissions '042608f6-317d-43b7-8fad-4618705abfcc=Role' 
}