param (
    $groupName
)

Write-Host "Ensuring AAD group with name '$groupName' exists"

$group = (az ad group list --filter "displayName eq '$groupName'") | ConvertFrom-Json
if ($group.Count -eq 0) {
    Write-Host "AAD Group '$groupName' does not exist, creating."
    az ad group create --display-name "$groupName" --mail-nickname "$groupName"
}
else {
    Write-Host "AAD Group '$groupName' already exists, doing nothing."
}