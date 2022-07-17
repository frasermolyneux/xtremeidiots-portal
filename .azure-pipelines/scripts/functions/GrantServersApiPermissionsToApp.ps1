param (
    $principalId,
    $environment
)

$serversApiName = "portal-servers-api-$environment"
$serversApiId = (az ad app list --filter "displayName eq '$serversApiName'" --query '[].appId') | ConvertFrom-Json
$serversApiSpnId = (az ad sp list --filter "appId eq '$serversApiId'" --query '[0].id') | ConvertFrom-Json

$permissions = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments) | ConvertFrom-Json
if ($null -eq ($permissions.value | Where-Object { $_.appRoleId -eq '6336ebcb-6b93-4abf-801e-7b485b77c142' })) {
    az rest -m POST -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments -b "{'principalId': '$principalId', 'resourceId': '$serversApiSpnId','appRoleId': '6336ebcb-6b93-4abf-801e-7b485b77c142'}"
}