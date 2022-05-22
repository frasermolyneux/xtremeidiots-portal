param (
    $environment
)

$servicePrincipalId = $env:servicePrincipalId

$spnMember = (az ad group member check --group "sg-sql-portal-$environment-admins" --member-id $servicePrincipalId) | ConvertFrom-Json
if ($spnMember.value -eq $false) {
    az ad group member add --group "sg-sql-portal-$environment-admins" --member-id $servicePrincipalId
}