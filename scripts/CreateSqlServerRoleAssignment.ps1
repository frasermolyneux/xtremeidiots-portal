param (
    $sqlServerName,
    $resourceGroup
)

$sqlServer = az sql server show --name $sqlServerName --resource-group $resourceGroup | ConvertFrom-Json
$principalId = $sqlServer.identity.principalId

$assignments = (az rest -m GET -u "https://graph.microsoft.com/v1.0/roleManagement/directory/roleAssignments?`$filter=principalId eq '$principalId'") | ConvertFrom-Json
if ($assignments.value.count -eq 0)
{
    az rest -m POST -u https://graph.microsoft.com/v1.0/roleManagement/directory/roleAssignments -b "{'@odata.type': '#microsoft.graph.unifiedRoleAssignment', 'principalId': '$principalId', 'roleDefinitionId': '88d8e3e3-8f55-4a1e-953a-9b9898b8876b','directoryScopeId': '/'}"
}