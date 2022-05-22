param (
    $environment
)

# Server Admin Group
. "./scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-portal-$environment-admins"

# PortalDb Groups
. "./scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-portal-$environment-portaldb-readers"
. "./scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-portal-$environment-portaldb-writers"

# IdentityDb Groups
. "./scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-portal-$environment-identitydb-readers"
. "./scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-portal-$environment-identitydb-writers"