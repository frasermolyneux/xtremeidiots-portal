param (
    $environment
)

$servicePrincipalId = '1f0d4d49-754e-4d18-88f4-c0a3a6d5d6fe'

$spnMember = (az ad group member check --group "sg-sql-portal-$environment-admins" --member-id $servicePrincipalId) | ConvertFrom-Json
if ($spnMember.value -eq $false) {
    az ad group member add --group "sg-sql-portal-$environment-admins" --member-id $servicePrincipalId
}