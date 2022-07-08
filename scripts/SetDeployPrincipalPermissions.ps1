param (
    $environment
)

$servicePrincipalId = '67692ae9-0f44-43bb-b979-f95656212586'

$spnMember = (az ad group member check --group "sg-sql-portal-$environment-admins" --member-id $servicePrincipalId) | ConvertFrom-Json
if ($spnMember.value -eq $false) {
    az ad group member add --group "sg-sql-portal-$environment-admins" --member-id $servicePrincipalId
}