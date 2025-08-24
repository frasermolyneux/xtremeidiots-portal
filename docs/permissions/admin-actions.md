# Admin Actions Permissions Matrix

Source: `AuthPolicies`, `AdminActionsAuthHandler`, `BaseAuthorizationHelper`

Legend:
- ✓ Granted (scope explained where needed)
- O (Owner) Requires the user to be the owner (creator/adminId match) of the admin action
- g(Game) Applies only for game types where the user holds the game-scoped claim
- S SeniorAdmin implicit override (has all)

Roles / Claims considered:
- SeniorAdmin (global)
- HeadAdmin (per game type value)
- GameAdmin (per game type value)
- Moderator (per game type value)

Admin Action Types (where relevant): Observation, Warning, Kick, TempBan, Ban.

| Policy / Capability                                   | SeniorAdmin | HeadAdmin (game) | GameAdmin (game) | Moderator (game) | Notes                                                                            |
| ----------------------------------------------------- | ----------- | ---------------- | ---------------- | ---------------- | -------------------------------------------------------------------------------- |
| Access Admin Actions (`AccessAdminActionsController`) | ✓           | ✓ g              | ✓ g              | ✓ g              | All admin levels                                                                 |
| Create Admin Action - Observation                     | ✓           | ✓ g              | ✓ g              | ✓ g              | Moderator-level action allowed for moderator/game admin/head admin of game       |
| Create Admin Action - Warning                         | ✓           | ✓ g              | ✓ g              | ✓ g              | Same as above                                                                    |
| Create Admin Action - Kick                            | ✓           | ✓ g              | ✓ g              | ✓ g              | Same as above                                                                    |
| Create Admin Action - TempBan                         | ✓           | ✓ g              | ✓ g              | ✗                | Moderator not permitted                                                          |
| Create Admin Action - Ban                             | ✓           | ✓ g              | ✓ g              | ✗                | Moderator not permitted                                                          |
| Edit Admin Action - Observation                       | ✓           | ✓ g              | O g              | O g              | Owner required for non-senior/head (handler grants head admin without ownership) |
| Edit Admin Action - Warning                           | ✓           | ✓ g              | O g              | O g              | Same rule                                                                        |
| Edit Admin Action - Kick                              | ✓           | ✓ g              | O g              | O g              | Same rule                                                                        |
| Edit Admin Action - TempBan                           | ✓           | ✓ g              | O g              | ✗                | GameAdmin owner only                                                             |
| Edit Admin Action - Ban                               | ✓           | ✓ g              | O g              | ✗                | GameAdmin owner only                                                             |
| Delete Admin Action (`DeleteAdminAction`)             | ✓           | ✗                | ✗                | ✗                | Only SeniorAdmin                                                                 |
| Change Admin Action Admin (`ChangeAdminActionAdmin`)  | ✓           | ✓ g              | ✗                | ✗                | Senior or HeadAdmin for game                                                     |
| Claim Admin Action (`ClaimAdminAction`)               | ✓           | ✓ g              | ✓ g              | ✗                | Senior or GameAdmin (head admin counts via game admin check)                     |
| Lift Admin Action (`LiftAdminAction`)                 | ✓           | ✓ g              | O g              | ✗                | GameAdmin must be owner; HeadAdmin no ownership constraint                       |
| Create Admin Action Topic (`CreateAdminActionTopic`)  | ✓           | ✓ g              | ✓ g              | ✗                | Senior or GameAdmin (head admin counts)                                          |

## Implementation Mapping
- Ownership determined via `XtremeIdiotsId` claim vs adminId on resource.
- Moderator-level action types: Observation, Warning, Kick.
- HeadAdmin inherits GameAdmin behaviours plus elevated edit/lift without ownership.

## Gaps / Potential Adjustments
- No explicit head admin restriction on edit (head can edit any action in game) - intentional?
- Delete restricted solely to SeniorAdmin.

