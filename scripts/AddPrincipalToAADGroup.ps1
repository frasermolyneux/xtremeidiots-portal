param (
    $principalId,
    $groupName
)

Write-Host "Ensuring principal with id '$principalId' is a member of '$groupName'"

$member = (az ad group member check --group $groupName --member-id $principalId) | ConvertFrom-Json
if ($member.value -eq $false) {
    Write-Host "Adding principal with id '$principalId' as a member of '$groupName'"
    az ad group member add --group $groupName --member-id $principalId
}
else {
    Write-Host "Principal with id '$principalId' is already a member of '$groupName' - doing nothing"
}