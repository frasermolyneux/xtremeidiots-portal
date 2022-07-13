param (
    $principalId,
    $environment
)

$repositoryApiName = "portal-repository-api-$environment"
$repositoryApiId = (az ad app list --filter "displayName eq '$repositoryApiName'" --query '[].appId') | ConvertFrom-Json
$repositoryApiSpnId = (az ad sp list --filter "appId eq '$repositoryApiId'" --query '[0].objectId') | ConvertFrom-Json

$permissions = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments) | ConvertFrom-Json
if ($null -eq ($permissions.value | Where-Object {$_.appRoleId -eq '6f8ce341-8bfa-4af9-af46-4f868f43a0ce'})) {
    az rest -m POST -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments -b "{'principalId': '$principalId', 'resourceId': '$repositoryApiSpnId','appRoleId': '6f8ce341-8bfa-4af9-af46-4f868f43a0ce'}"
}