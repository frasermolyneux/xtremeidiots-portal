param (
    $principalId,
    $applicationName,
    $appRole
)

$apiId = (az ad app list --filter "displayName eq '$applicationName'" --query '[].appId') | ConvertFrom-Json
$resourceId = (az ad sp list --filter "appId eq '$apiId'" --query '[0].id') | ConvertFrom-Json
$apiSpn = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$resourceId) | ConvertFrom-Json
$appRoleId = ($apiSpn.appRoles | Where-Object { $_.displayName -eq "$appRole" }).id

$permissions = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments) | ConvertFrom-Json
if ($null -eq ($permissions.value | Where-Object { $_.appRoleId -eq $appRoleId })) {
    az rest -m POST -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments -b "{'principalId': '$principalId', 'resourceId': '$resourceId','appRoleId': '$appRoleId'}"
}
