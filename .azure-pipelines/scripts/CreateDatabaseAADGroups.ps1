param (
    $environment
)

# PortalDb Groups
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-portaldb-$environment-readers"
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-portaldb-$environment-writers"

# IdentityDb Groups
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-readers"
. "./.azure-pipelines/scripts/functions/CreateAADGroup.ps1" -groupName "sg-sql-platform-$environment-portalidentitydb-$environment-writers"