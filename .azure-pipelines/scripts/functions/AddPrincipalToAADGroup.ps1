param (
    $principalId,
    $groupName
)

$member = (az ad group member check --group $groupName --member-id $principalId) | ConvertFrom-Json
if ($member.value -eq $false) {
    az ad group member add --group $groupName --member-id $principalId
}