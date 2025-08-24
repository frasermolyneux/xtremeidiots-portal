# Credentials Permissions Matrix

Source: `AuthPolicies`, `CredentialsAuthHandler`, `GameServersAuthHandler`, `BaseAuthorizationHelper`, `Views/Credentials/Index.cshtml`

Legend:
- ✓ Granted
- (all) All servers
- g Game-type scope
- s Specific server (claim value is server GUID)
- ✗ Not granted

Roles / Claims considered (minimal single-claim scenarios):
- SeniorAdmin
- HeadAdmin (game)
- GameAdmin (game)
- FtpCredentials (server)
- RconCredentials (server)
- LiveRcon (game) (included due to RCON credential view logic)

| Capability / Policy | SeniorAdmin | HeadAdmin (game) | GameAdmin (game) | FtpCredentials (server) | RconCredentials (server) | LiveRcon (game) | Notes |
|---------------------|------------|------------------|------------------|-------------------------|--------------------------|-----------------|-------|
| Access Credentials Page (`AccessCredentials`) | ✓ (all) | ✓ g | ✓ g | ✓ s | ✓ s | ✗ | LiveRcon NOT in access claim group |
| View FTP Hostname (`ViewFtpCredential`) | ✓ (all) | ✓ g | ✗ | ✓ s | ✗ | ✗ | HeadAdmin or per-server FTP claim |
| View FTP Username (`ViewFtpCredential`) | ✓ (all) | ✓ g | ✗ | ✓ s | ✗ | ✗ | Same rule |
| View FTP Password (`ViewFtpCredential`) | ✓ (all) | ✓ g | ✗ | ✓ s | ✗ | ✗ | Same rule |
| View RCON Password (`ViewRconCredential`) | ✓ (all) | ✗ | ✓ g | ✗ | ✗ | ✓ g | Handler: Senior OR (GameAdmin/LiveRcon) — HeadAdmin missing |

## Nuances
- HeadAdmin omission in `ViewRconCredential` likely unintentional asymmetry vs FTP.
- `RconCredentials` claim not checked in handler (has page access only).
- `LiveRcon` cannot reach page alone (not in AccessCredentials claim set) though it enables viewing RCON.

## Potential Alignments
- Add HeadAdmin + per-server RconCredentials to `ViewRconCredential` logic.
- Include LiveRcon in `AccessCredentials` claim set if appropriate.

