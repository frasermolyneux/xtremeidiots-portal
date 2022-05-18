$identity = az functionapp identity show --name fn-servers-portal-prd-uksouth-01 --resource-group rg-portal-prd-uksouth-01 | ConvertFrom-Json
$principalId = $identity.principalId

# Repository API
$repositoryApiName = 'portal-repository-api-prd'
$repositoryApiId = (az ad app list --filter "displayName eq '$repositoryApiName'" --query '[].appId') | ConvertFrom-Json
$repositoryApiSpnId = (az ad sp list --filter "appId eq '$repositoryApiId'" --query '[0].objectId') | ConvertFrom-Json

$permissions = (az rest -m GET -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments) | ConvertFrom-Json
if ($permissions.value.count -eq 0) {
    az rest -m POST -u https://graph.microsoft.com/v1.0/servicePrincipals/$principalId/appRoleAssignments -b "{'principalId': '$principalId', 'resourceId': '$repositoryApiSpnId','appRoleId': '6f8ce341-8bfa-4af9-af46-4f868f43a0ce'}"
}