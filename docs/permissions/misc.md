# Miscellaneous Permissions Matrix

Source: `AuthPolicies`, relevant handlers, `BaseAuthorizationHelper`

Legend:
- ✓ Granted
- g Game-type scope where applicable
- ✗ Not granted

Roles / Claims (subset):
- SeniorAdmin
- HeadAdmin (game)
- GameAdmin (game)
- Moderator (game)
- BanFileMonitor (server)
- LiveRcon (game)

| Capability / Policy | SeniorAdmin | HeadAdmin (game) | GameAdmin (game) | Moderator (game) | BanFileMonitor (server) | LiveRcon (game) | Notes |
|---------------------|------------|------------------|------------------|------------------|-------------------------|-----------------|-------|
| Access Home (`AccessHome`) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Assumed all authenticated roles (handler check not shown) |
| Access Profile (`AccessProfile`) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Likely any authenticated user (placeholder) |
| Access Change Log (`AccessChangeLog`) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Handler simple claim group or open access |
| Access Status (`AccessStatus`) | ✓ | ✓ | ✓ | ✗ | ✓ | ✗ | StatusAccessLevels: includes BanFileMonitor, excludes Moderator & LiveRcon |
| Access Servers (`AccessServers`) | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ServersAuthHandler unconditionally succeeds (public to logged-in) |
| Access Migration (`AccessMigration`) | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | Senior only |

## Notes
- Some entries (Home/Profile) may in reality allow anonymous; adjust if implementation differs.
- LiveRcon limited scope; not in StatusAccessLevels.

