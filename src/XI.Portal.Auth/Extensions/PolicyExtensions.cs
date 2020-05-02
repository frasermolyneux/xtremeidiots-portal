using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.BanFileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Credentials.AuthorizationRequirements;
using XI.Portal.Auth.Demos.AuthorizationRequirements;
using XI.Portal.Auth.FileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;
using XI.Portal.Auth.Home.AuthorizationRequirements;
using XI.Portal.Auth.Maps.AuthorizationRequirements;
using XI.Portal.Auth.Players.AuthorizationRequirements;
using XI.Portal.Auth.RconMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Servers.AuthorizationRequirements;

namespace XI.Portal.Auth.Extensions
{
    public static class PolicyExtensions
    {
        public static void AddXtremeIdiotsPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(AuthPolicies.RootPolicy, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.SeniorAdmin)
            );

            options.AddPolicy(AuthPolicies.ServersManagement, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin || claim.Type == XtremeIdiotsClaimTypes.HeadAdmin
                )));

            options.AddPolicy(AuthPolicies.PlayersManagement, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            options.AddPolicy(AuthPolicies.UserHasCredentials, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                        claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                                 claim.Type == PortalClaimTypes.FtpCredentials ||
                                 claim.Type == PortalClaimTypes.RconCredentials
                    )
                )
            );

            options.AddPolicy(AuthPolicies.ViewServiceStatus, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                        claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                    )
                )
            );

            options.AddPolicy(AuthPolicies.AccessServerAdmin, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            options.AddPolicy(AuthPolicies.AccessGlobalChatLog, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                )));

            options.AddPolicy(AuthPolicies.CanAccessGameChatLog, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                )));

            options.AddPolicy(AuthPolicies.AccessLiveRcon, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            // Admin Actions
            options.AddPolicy(AuthPolicies.AccessAdminActionsController, policy => { policy.Requirements.Add(new AccessAdminActions()); });
            options.AddPolicy(AuthPolicies.ChangeAdminActionAdmin, policy => { policy.Requirements.Add(new ChangeAdminActionAdmin()); });
            options.AddPolicy(AuthPolicies.ClaimAdminAction, policy => { policy.Requirements.Add(new ClaimAdminAction()); });
            options.AddPolicy(AuthPolicies.CreateAdminAction, policy => { policy.Requirements.Add(new CreateAdminAction()); });
            options.AddPolicy(AuthPolicies.CreateAdminActionTopic, policy => { policy.Requirements.Add(new CreateAdminActionTopic()); });
            options.AddPolicy(AuthPolicies.DeleteAdminAction, policy => { policy.Requirements.Add(new DeleteAdminAction()); });
            options.AddPolicy(AuthPolicies.EditAdminAction, policy => { policy.Requirements.Add(new EditAdminAction()); });
            options.AddPolicy(AuthPolicies.LiftAdminAction, policy => { policy.Requirements.Add(new LiftAdminAction()); });

            // Ban File Monitors
            options.AddPolicy(AuthPolicies.AccessBanFileMonitors, policy => { policy.Requirements.Add(new AccessBanFileMonitors()); });
            options.AddPolicy(AuthPolicies.CreateBanFileMonitor, policy => { policy.Requirements.Add(new CreateBanFileMonitor()); });
            options.AddPolicy(AuthPolicies.ViewBanFileMonitor, policy => { policy.Requirements.Add(new ViewBanFileMonitor()); });
            options.AddPolicy(AuthPolicies.EditBanFileMonitor, policy => { policy.Requirements.Add(new EditBanFileMonitor()); });
            options.AddPolicy(AuthPolicies.DeleteBanFileMonitor, policy => { policy.Requirements.Add(new DeleteBanFileMonitor()); });

            // Credentials
            options.AddPolicy(AuthPolicies.AccessCredentials, policy => { policy.Requirements.Add(new AccessCredentials()); });

            // Demos
            options.AddPolicy(AuthPolicies.AccessDemos, policy => { policy.Requirements.Add(new AccessDemos()); });
            options.AddPolicy(AuthPolicies.DeleteDemo, policy => { policy.Requirements.Add(new DeleteDemo()); });

            // Ban File Monitors
            options.AddPolicy(AuthPolicies.AccessFileMonitors, policy => { policy.Requirements.Add(new AccessFileMonitors()); });
            options.AddPolicy(AuthPolicies.CreateFileMonitor, policy => { policy.Requirements.Add(new CreateFileMonitor()); });
            options.AddPolicy(AuthPolicies.ViewFileMonitor, policy => { policy.Requirements.Add(new ViewFileMonitor()); });
            options.AddPolicy(AuthPolicies.EditFileMonitor, policy => { policy.Requirements.Add(new EditFileMonitor()); });
            options.AddPolicy(AuthPolicies.DeleteFileMonitor, policy => { policy.Requirements.Add(new DeleteFileMonitor()); });

            // Game Servers
            options.AddPolicy(AuthPolicies.AccessGameServers, policy => { policy.Requirements.Add(new AccessGameServers()); });
            options.AddPolicy(AuthPolicies.CreateGameServer, policy => { policy.Requirements.Add(new CreateGameServer()); });
            options.AddPolicy(AuthPolicies.DeleteGameServer, policy => { policy.Requirements.Add(new DeleteGameServer()); });
            options.AddPolicy(AuthPolicies.EditGameServer, policy => { policy.Requirements.Add(new EditGameServer()); });
            options.AddPolicy(AuthPolicies.EditGameServerFtp, policy => { policy.Requirements.Add(new EditGameServerFtp()); });
            options.AddPolicy(AuthPolicies.EditGameServerRcon, policy => { policy.Requirements.Add(new EditGameServerRcon()); });
            options.AddPolicy(AuthPolicies.ViewFtpCredential, policy => { policy.Requirements.Add(new ViewFtpCredential()); });
            options.AddPolicy(AuthPolicies.ViewGameServer, policy => { policy.Requirements.Add(new ViewGameServer()); });
            options.AddPolicy(AuthPolicies.ViewRconCredential, policy => { policy.Requirements.Add(new ViewRconCredential()); });

            // Home
            options.AddPolicy(AuthPolicies.AccessHome, policy => { policy.Requirements.Add(new AccessHome()); });

            // Maps
            options.AddPolicy(AuthPolicies.AccessMaps, policy => { policy.Requirements.Add(new AccessMaps()); });

            // Players
            options.AddPolicy(AuthPolicies.AccessPlayers, policy => { policy.Requirements.Add(new AccessPlayers()); });

            // Rcon Monitors
            options.AddPolicy(AuthPolicies.AccessRconMonitors, policy => { policy.Requirements.Add(new AccessRconMonitors()); });
            options.AddPolicy(AuthPolicies.CreateRconMonitor, policy => { policy.Requirements.Add(new CreateRconMonitor()); });
            options.AddPolicy(AuthPolicies.ViewRconMonitor, policy => { policy.Requirements.Add(new ViewRconMonitor()); });
            options.AddPolicy(AuthPolicies.EditRconMonitor, policy => { policy.Requirements.Add(new EditRconMonitor()); });
            options.AddPolicy(AuthPolicies.DeleteRconMonitor, policy => { policy.Requirements.Add(new DeleteRconMonitor()); });

            // Servers
            options.AddPolicy(AuthPolicies.AccessServers, policy => { policy.Requirements.Add(new AccessServers()); });
        }
    }
}