# Ban File Monitors Permissions Matrix

Source: `AuthPolicies`, `BanFileMonitorsAuthHandler`, `BaseAuthorizationHelper`

Legend:
- ✓ Granted
- g Game-type scope
- s Server (GUID) scope
- ✗ Not granted

Roles / Claims:
- SeniorAdmin
- HeadAdmin (game)
- BanFileMonitor (server)

| Capability / Policy                                | SeniorAdmin | HeadAdmin (game) | BanFileMonitor (server) | Notes                                                                             |
| -------------------------------------------------- | ----------- | ---------------- | ----------------------- | --------------------------------------------------------------------------------- |
| Access Ban File Monitors (`AccessBanFileMonitors`) | ✓           | ✓ g              | ✓ s                     | Claim group includes all three                                                    |
| Create Ban File Monitor (`CreateBanFileMonitor`)   | ✓           | ✓ g              | ✓ s                     | Senior or (HeadAdmin for game) OR (BanFileMonitor for server) via composite check |
| View Ban File Monitor (`ViewBanFileMonitor`)       | ✓           | ✓ g              | ✓ s                     | Same composite                                                                    |
| Edit Ban File Monitor (`EditBanFileMonitor`)       | ✓           | ✓ g              | ✓ s                     | Same composite                                                                    |
| Delete Ban File Monitor (`DeleteBanFileMonitor`)   | ✓           | ✓ g              | ✓ s                     | Same composite                                                                    |

## Notes
- No explicit requirement for possessing both server and game claims; either suffices.
- HeadAdmin acts game-wide; per-server claim limited.

