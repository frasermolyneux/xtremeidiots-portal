# Maps / Map Management Permissions Matrix

Source: `AuthPolicies`, `ServerAdminAuthHandler`, `MapsAuthHandler`, `BaseAuthorizationHelper`

Legend:
- ✓ Granted
- g Game-type scope
- ✗ Not granted

Roles / Claims:
- SeniorAdmin
- HeadAdmin (game)
- GameAdmin (game)
- Moderator (game)

| Capability / Policy | SeniorAdmin | HeadAdmin (game) | GameAdmin (game) | Moderator (game) | Notes |
|---------------------|------------|------------------|------------------|------------------|-------|
| Access Maps (`AccessMaps`) | ✓ | ✓ g | ✓ g | ✓ g | All admin levels via handler (not shown here if simple) |
| Manage Maps (`ManageMaps`) | ✓ | ✓ g | ✗ | ✗ | Senior & Head only |
| Access Map Manager Controller (`AccessMapManagerController`) | ✓ | ✓ g | ✗ | ✗ | Same group |
| Create Map Pack (`CreateMapPack`) | ✓ | ✓ g | ✗ | ✗ | Senior & Head only |
| Edit Map Pack (`EditMapPack`) | ✓ | ✓ g | ✗ | ✗ | Same group |
| Delete Map Pack (`DeleteMapPack`) | ✓ | ✓ g | ✗ | ✗ | Same group |
| Push Map To Remote (`PushMapToRemote`) | ✓ | ✓ g | ✗ | ✗ | Same group |
| Delete Map From Host (`DeleteMapFromHost`) | ✓ | ✓ g | ✗ | ✗ | Same group |

## Notes
- GameAdmin / Moderator limited to viewing maps (no management actions).

