$environment = 'prd'
$databaseName = 'portaldb'

$servicePrincipalId = '1f0d4d49-754e-4d18-88f4-c0a3a6d5d6fe'

$adminGroupName = "sg-sql-portal-$environment-admins"
$readersGroupName = "sg-sql-portal-$environment-$databaseName-readers"
$writersGroupName = "sg-sql-portal-$environment-$databaseName-writers"

$serverAdminGroup = (az ad group list --filter "displayName eq '$adminGroupName'") | ConvertFrom-Json
if ($serverAdminGroup.Count -eq 0) {
    az ad group create --display-name $adminGroupName --mail-nickname $adminGroupName
}

$readersGroup = (az ad group list --filter "displayName eq '$readersGroupName'") | ConvertFrom-Json
if ($readersGroup.Count -eq 0) {
    az ad group create --display-name $readersGroupName --mail-nickname $readersGroupName
}

$writersGroup = (az ad group list --filter "displayName eq '$writersGroupName'") | ConvertFrom-Json
if ($writersGroup.Count -eq 0) {
    az ad group create --display-name $writersGroupName --mail-nickname $writersGroupName
}

$spnMember = (az ad group member check --group $adminGroupName --member-id $servicePrincipalId) | ConvertFrom-Json
if ($spnMember.value -eq $false) {
    az ad group member add --group $adminGroupName --member-id $servicePrincipalId
}