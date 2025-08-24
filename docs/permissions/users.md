# Users / User Management Permissions Matrix

Source: `AuthPolicies`, `UsersAuthHandler`, `BaseAuthorizationHelper`

Legend:
- ✓ Granted
- ✗ Not granted

Roles / Claims:
- SeniorAdmin
- HeadAdmin (game)
- GameAdmin (game)

(Moderator intentionally excluded: no user management capabilities implemented.)

| Capability / Policy                       | SeniorAdmin | HeadAdmin (game) | GameAdmin (game) | Notes                                                       |
| ----------------------------------------- | ----------- | ---------------- | ---------------- | ----------------------------------------------------------- |
| Access Users (`AccessUsers`)              | ✓           | ✓                | ✓                | Handler allows all admin levels except moderators (assumed) |
| Perform User Search (`PerformUserSearch`) | ✓           | ✓                | ✓                | Same as access                                              |
| Create User Claim (`CreateUserClaim`)     | ✓           | ✓                | ✗                | HeadAdmin allowed; GameAdmin excluded                       |
| Delete User Claim (`DeleteUserClaim`)     | ✓           | ✓                | ✗                | HeadAdmin allowed; GameAdmin excluded                       |

## Notes
- GameAdmin cannot create/delete user permission claims.
- Scope for HeadAdmin is not game-limited for user claim operations (global effect) — potential design decision.

