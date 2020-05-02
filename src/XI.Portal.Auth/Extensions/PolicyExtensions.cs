using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.BanFileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Credentials.AuthorizationRequirements;
using XI.Portal.Auth.Demos.AuthorizationRequirements;
using XI.Portal.Auth.FileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;
using XI.Portal.Auth.RconMonitors.AuthorizationRequirements;
using XI.Portal.Auth.Servers.AuthorizationRequirements;

namespace XI.Portal.Auth.Extensions
{
    public static class PolicyExtensions
    {
        public static void AddXtremeIdiotsPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(XtremeIdiotsPolicy.RootPolicy, policy =>
                policy.RequireClaim(XtremeIdiotsClaimTypes.SeniorAdmin)
            );

            options.AddPolicy(XtremeIdiotsPolicy.ServersManagement, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin || claim.Type == XtremeIdiotsClaimTypes.HeadAdmin
                )));

            options.AddPolicy(XtremeIdiotsPolicy.PlayersManagement, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            options.AddPolicy(XtremeIdiotsPolicy.UserHasCredentials, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                        claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                                 claim.Type == PortalClaimTypes.FtpCredentials ||
                                 claim.Type == PortalClaimTypes.RconCredentials
                    )
                )
            );

            options.AddPolicy(XtremeIdiotsPolicy.ViewServiceStatus, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                        claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                                 claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                    )
                )
            );

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessServerAdmin, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessGlobalChatLog, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                )));

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessGameChatLog, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin
                )));

            options.AddPolicy(XtremeIdiotsPolicy.CanAccessLiveRcon, policy =>
                policy.RequireAssertion(context => context.User.HasClaim(
                    claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.HeadAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.GameAdmin ||
                             claim.Type == XtremeIdiotsClaimTypes.Moderator
                )));

            // Admin Actions
            options.AddPolicy(XtremeIdiotsPolicy.AccessAdminActionsController, policy => { policy.Requirements.Add(new AccessAdminActions()); });
            options.AddPolicy(XtremeIdiotsPolicy.ChangeAdminActionAdmin, policy => { policy.Requirements.Add(new ChangeAdminActionAdmin()); });
            options.AddPolicy(XtremeIdiotsPolicy.ClaimAdminAction, policy => { policy.Requirements.Add(new ClaimAdminAction()); });
            options.AddPolicy(XtremeIdiotsPolicy.CreateAdminAction, policy => { policy.Requirements.Add(new CreateAdminAction()); });
            options.AddPolicy(XtremeIdiotsPolicy.CreateAdminActionTopic, policy => { policy.Requirements.Add(new CreateAdminActionTopic()); });
            options.AddPolicy(XtremeIdiotsPolicy.DeleteAdminAction, policy => { policy.Requirements.Add(new DeleteAdminAction()); });
            options.AddPolicy(XtremeIdiotsPolicy.EditAdminAction, policy => { policy.Requirements.Add(new EditAdminAction()); });
            options.AddPolicy(XtremeIdiotsPolicy.LiftAdminAction, policy => { policy.Requirements.Add(new LiftAdminAction()); });

            // Ban File Monitors
            options.AddPolicy(XtremeIdiotsPolicy.AccessBanFileMonitors, policy => { policy.Requirements.Add(new AccessBanFileMonitors()); });
            options.AddPolicy(XtremeIdiotsPolicy.CreateBanFileMonitor, policy => { policy.Requirements.Add(new CreateBanFileMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.ViewBanFileMonitor, policy => { policy.Requirements.Add(new ViewBanFileMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.EditBanFileMonitor, policy => { policy.Requirements.Add(new EditBanFileMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.DeleteBanFileMonitor, policy => { policy.Requirements.Add(new DeleteBanFileMonitor()); });

            // Credentials
            options.AddPolicy(XtremeIdiotsPolicy.AccessCredentials, policy => { policy.Requirements.Add(new AccessCredentials()); });

            // Demos
            options.AddPolicy(XtremeIdiotsPolicy.AccessDemos, policy => { policy.Requirements.Add(new AccessDemos()); });
            options.AddPolicy(XtremeIdiotsPolicy.DeleteDemo, policy => { policy.Requirements.Add(new DeleteDemo()); });

            // Ban File Monitors
            options.AddPolicy(XtremeIdiotsPolicy.AccessFileMonitors, policy => { policy.Requirements.Add(new AccessFileMonitors()); });
            options.AddPolicy(XtremeIdiotsPolicy.CreateFileMonitor, policy => { policy.Requirements.Add(new CreateFileMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.ViewFileMonitor, policy => { policy.Requirements.Add(new ViewFileMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.EditFileMonitor, policy => { policy.Requirements.Add(new EditFileMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.DeleteFileMonitor, policy => { policy.Requirements.Add(new DeleteFileMonitor()); });

            // Game Servers
            options.AddPolicy(XtremeIdiotsPolicy.AccessGameServers, policy => { policy.Requirements.Add(new AccessGameServers()); });
            options.AddPolicy(XtremeIdiotsPolicy.CreateGameServer, policy => { policy.Requirements.Add(new CreateGameServer()); });
            options.AddPolicy(XtremeIdiotsPolicy.DeleteGameServer, policy => { policy.Requirements.Add(new DeleteGameServer()); });
            options.AddPolicy(XtremeIdiotsPolicy.EditGameServer, policy => { policy.Requirements.Add(new EditGameServer()); });
            options.AddPolicy(XtremeIdiotsPolicy.EditGameServerFtp, policy => { policy.Requirements.Add(new EditGameServerFtp()); });
            options.AddPolicy(XtremeIdiotsPolicy.EditGameServerRcon, policy => { policy.Requirements.Add(new EditGameServerRcon()); });
            options.AddPolicy(XtremeIdiotsPolicy.ViewFtpCredential, policy => { policy.Requirements.Add(new ViewFtpCredential()); });
            options.AddPolicy(XtremeIdiotsPolicy.ViewGameServer, policy => { policy.Requirements.Add(new ViewGameServer()); });
            options.AddPolicy(XtremeIdiotsPolicy.ViewRconCredential, policy => { policy.Requirements.Add(new ViewRconCredential()); });

            // Rcon Monitors
            options.AddPolicy(XtremeIdiotsPolicy.AccessRconMonitors, policy => { policy.Requirements.Add(new AccessRconMonitors()); });
            options.AddPolicy(XtremeIdiotsPolicy.CreateRconMonitor, policy => { policy.Requirements.Add(new CreateRconMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.ViewRconMonitor, policy => { policy.Requirements.Add(new ViewRconMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.EditRconMonitor, policy => { policy.Requirements.Add(new EditRconMonitor()); });
            options.AddPolicy(XtremeIdiotsPolicy.DeleteRconMonitor, policy => { policy.Requirements.Add(new DeleteRconMonitor()); });

            // Servers
            options.AddPolicy(XtremeIdiotsPolicy.AccessServers, policy => { policy.Requirements.Add(new AccessServers()); });
        }
    }
}