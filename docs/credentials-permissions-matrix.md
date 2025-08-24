# Credentials View Permissions Matrix

Generated: 2025-08-24

Source components reviewed:
* `Views/Credentials/Index.cshtml`
* Associated authorization policies in `AuthPolicies`
* Helper logic in authorization handlers (e.g. `BaseAuthorizationHelper`)

Scope: This matrix focuses ONLY on the game server credentials view (listing and revealing RCON / FTP credentials). It documents the minimal single claim or role required for each capability. Real users may hold multiple claims; effective access is the union of their claims.

Legend:
* ✓ Granted
* ✗ Not granted
* (all) Applies to all servers
* (g) Applies only to servers of game types where the user has that game-type claim
* (s) Applies only to the specific server(s) where the user holds that server-scoped claim (matching GUID)

Rows included are only the roles/claims influencing this page. (Moderator, GameServer, etc. are excluded because they don't affect credential visibility here.)

| Role / Claim (minimal case) | Access Credentials Page (`AccessCredentials`) | See Server Listed | View RCON Password (`ViewRconCredential`) | View FTP Hostname (`ViewFtpCredential`) | View FTP Username | View FTP Password |
|-----------------------------|----------------------------------------------|-------------------|-------------------------------------------|-----------------------------------------|-------------------|-------------------|
| SeniorAdmin                 | ✓ (all)                                      | ✓ (all)           | ✓ (all)                                   | ✓ (all)                                 | ✓ (all)           | ✓ (all)           |
| HeadAdmin (game type)       | ✓                                            | ✓ (g)             | ✗ (handler doesn’t allow)                 | ✓ (g)                                   | ✓ (g)             | ✓ (g)             |
| GameAdmin (game type)       | ✓                                            | ✓ (g)             | ✓ (g)                                     | ✗                                       | ✗                 | ✗                 |
| FtpCredentials (server)     | ✓                                            | ✓ (s)             | ✗                                         | ✓ (s)                                   | ✓ (s)             | ✓ (s)             |
| RconCredentials (server)    | ✓                                            | ✓ (s)             | ✗ (claim not checked in handler)          | ✗                                       | ✗                 | ✗                 |
| LiveRcon (game type)        | ✗ (not in `CredentialsAccessLevels`)         | —                 | (Would be ✓ (g) if page accessible)       | ✗                                       | ✗                 | ✗                 |

## Notes / Nuances

1. HeadAdmin cannot view RCON credentials under current implementation (likely unintended asymmetry vs FTP access).
2. `RconCredentials` claim allows page access and server row visibility but NOT RCON password (handler ignores per‑server claim).
3. `LiveRcon` claim enables RCON viewing logic in its handler, but the user cannot reach the page with only that claim (it isn’t part of `AccessCredentials` policy claim set).
4. GameAdmin can view RCON but not FTP credentials (by design in handler logic).
5. Per‑server visibility (for `FtpCredentials` / `RconCredentials`) depends on the server’s GUID matching the claim value.
6. Empty FTP username/password cells in the UI may reflect either lack of permission (shows masked) or underlying empty stored value.

## Potential Improvements (Optional)

If alignment with likely intent / documentation is desired:

* Add HeadAdmin (and optionally per‑server `RconCredentials`) to the `ViewRconCredential` handler.
* Add `LiveRcon` to the `CredentialsAccessLevels` collection if those users should access the page at all.
* Consider symmetric handling: HeadAdmin → both FTP & RCON; GameAdmin → possibly RCON only (as today) or neither; document clearly.
* Update the user-facing permissions documentation to reflect current asymmetry if code is left unchanged.

## Change Management

This file is informational; modifying policies requires updates to the corresponding authorization handlers and potentially `PolicyExtensions` registration. After any code adjustment, re-run `dotnet build` and relevant tests to validate no regressions.
