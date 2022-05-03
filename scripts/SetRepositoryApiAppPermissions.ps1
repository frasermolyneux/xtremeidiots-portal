$appName = 'webapi-repository-portal-prd-uksouth-01'
$resourceGroup = 'rg-portal-prd-uksouth-01'
$readersGroupName = "sg-sql-portal-prd-portaldb-readers"
$writersGroupName = "sg-sql-portal-prd-portaldb-writers"

$webApp = (az webapp show --name $appName --resource-group $resourceGroup) | ConvertFrom-Json
$principalId = $webApp.identity.principalId

$readersMember = (az ad group member check --group $readersGroupName --member-id $principalId) | ConvertFrom-Json
if ($readersMember.value -eq $false) {
    az ad group member add --group $readersGroupName --member-id $principalId
}

$writersMember = (az ad group member check --group $writersGroupName --member-id $principalId) | ConvertFrom-Json
if ($writersMember.value -eq $false) {
    az ad group member add --group $writersGroupName --member-id $principalId
}
