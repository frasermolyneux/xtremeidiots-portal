# Server Administration Permissions Matrix

Source: `AuthPolicies`, `ServerAdminAuthHandler`, `BaseAuthorizationHelper`

Legend:
- ✓ Granted
- g Game-type scope
- ✗ Not granted

Roles / Claims:
- SeniorAdmin
- HeadAdmin (game)
- GameAdmin (game)
- Moderator (game)
- ServerAdmin (game) (additional admin-esque claim)
- LiveRcon (game)

| Capability / Policy | SeniorAdmin | HeadAdmin (game) | GameAdmin (game) | Moderator (game) | ServerAdmin (game) | LiveRcon (game) | Notes |
|---------------------|------------|------------------|------------------|------------------|--------------------|-----------------|-------|
| Access Server Admin (`AccessServerAdmin`) | ✓ | ✓ g | ✓ g | ✓ g | ✓ g | ✗ | Claim group includes ServerAdmin but excludes LiveRcon |
| Access Live RCON (`AccessLiveRcon`) | ✓ | ✓ g | ✓ g | ✗ | ✗ | ✓ g | LiveRcon group excludes Moderator & ServerAdmin |
| View Live RCON (`ViewLiveRcon`) | ✓ | ✓ g | ✓ g | ✗ | ✗ | ✓ g | Same as AccessLiveRcon composite |
| View Game Chat Log (`ViewGameChatLog`) | ✓ | ✓ g | ✓ g | ✗ | ✗ | ✗ | Senior or GameAdmin only (head admin passes via game admin check) |
| View Global Chat Log (`ViewGlobalChatLog`) | ✓ | ✓ g | ✓ g | ✗ | ✗ | ✗ | Excludes moderator/serveradmin/liveRcon |
| View Server Chat Log (`ViewServerChatLog`) | ✓ | ✓ g | ✓ g | ✓ g | ✗ | ✗ | Moderator permitted; ServerAdmin NOT included |
| Manage Maps (`ManageMaps`) | ✓ | ✓ g | ✗ | ✗ | ✗ | ✗ | Senior & Head only |
| Access Map Manager Controller (`AccessMapManagerController`) | ✓ | ✓ g | ✗ | ✗ | ✗ | ✗ | Same group |
| Push Map To Remote (`PushMapToRemote`) | ✓ | ✓ g | ✗ | ✗ | ✗ | ✗ | Same group |
| Delete Map From Host (`DeleteMapFromHost`) | ✓ | ✓ g | ✗ | ✗ | ✗ | ✗ | Same group |
| Lock Chat Messages (`LockChatMessages`) | ✓ | ✓ g | ✓ g | ✓ g | ✗ | ✗ | AdminLevelsExcludingModerators + moderator specific game check allows Moderator; ServerAdmin excluded |

## Notes
- ServerAdmin claim only grants AccessServerAdmin page; lacks many specific capabilities.
- LiveRcon limited to RCON access policies; cannot access broader ServerAdmin page.

