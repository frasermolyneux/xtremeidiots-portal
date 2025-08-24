# Game Servers Permissions Matrix

Source: `AuthPolicies`, `GameServersAuthHandler`, `BaseAuthorizationHelper`

Legend:
- ✓ Granted
- g Game-type scoped (claim value = game type)
- s Server ID scoped (claim value = server GUID)
- ✗ Not granted

Roles / Claims:
- SeniorAdmin
- HeadAdmin (game)
- GameServer (game) (game-type level management claim)
- GameAdmin (game) (included where handler allows via composite checks)
- LiveRcon (game) (for RCON view; already covered in credentials but listed here for completeness)

| Capability / Policy | SeniorAdmin | HeadAdmin (game) | GameServer (game) | GameAdmin (game) | LiveRcon (game) | Notes |
|---------------------|------------|------------------|-------------------|------------------|-----------------|-------|
| Access Game Servers (`AccessGameServers`) | ✓ | ✓ g | ✓ g | ✗ | ✗ | Access group excludes GameAdmin |
| Create Game Server (`CreateGameServer`) | ✓ | ✓ g | ✗ | ✗ | ✗ | Senior or HeadAdmin |
| Delete Game Server (`DeleteGameServer`) | ✓ | ✗ | ✗ | ✗ | ✗ | Senior only |
| Edit Game Server (General) (`EditGameServer`) | ✓ | ✓ g | ✓ g | ✗ | ✗ | HeadAdmin & GameServer claim via combined access |
| Edit Game Server FTP (`EditGameServerFtp`) | ✓ | ✓ g | ✗ | ✗ | ✗ | Senior or HeadAdmin |
| Edit Game Server RCON (`EditGameServerRcon`) | ✓ | ✓ g | ✗ | ✗ | ✗ | Senior or HeadAdmin |
| View Game Server (`ViewGameServer`) | ✓ | ✓ g | ✓ g | ✗ | ✗ | Combined access (HeadAdmin or GameServer) |
| View FTP Credential (`ViewFtpCredential`) | ✓ | ✓ g | ✗ | ✗ | ✗ | See credentials matrix for per-server claim nuance |
| View RCON Credential (`ViewRconCredential`) | ✓ | ✗ | ✗ | ✓ g | ✓ g | HeadAdmin missing; GameAdmin or LiveRcon allowed |

## Observations
- GameAdmin cannot access the Game Servers list unless also HeadAdmin or GameServer claim holder.
- `GameServer` claim grants broad edit/view within its game type (excluding FTP/RCON segments).
- RCON/FTP edits restricted to HeadAdmin (not GameServer claim holders).

