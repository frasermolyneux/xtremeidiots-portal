param (
    $environment
)

# PortalDb Groups
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-portaldb-readers"
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-portaldb-writers"

# IdentityDb Groups
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-identitydb-readers"
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-identitydb-writers"