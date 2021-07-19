using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.AdminActions.AuthorizationRequirements;
using XI.Portal.Auth.BanFileMonitors.AuthorizationRequirements;
using XI.Portal.Auth.ChangeLog.AuthorizationRequirements;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Credentials.AuthorizationRequirements;
using XI.Portal.Auth.Demos.AuthorizationRequirements;
using XI.Portal.Auth.GameServers.AuthorizationRequirements;
using XI.Portal.Auth.Home.AuthorizationRequirements;
using XI.Portal.Auth.Maps.AuthorizationRequirements;
using XI.Portal.Auth.Migration.AuthorizationRequirements;
using XI.Portal.Auth.Players.AuthorizationRequirements;
using XI.Portal.Auth.ServerAdmin.AuthorizationRequirements;
using XI.Portal.Auth.Servers.AuthorizationRequirements;
using XI.Portal.Auth.Status.AuthorizationRequirements;
using XI.Portal.Auth.Users.AuthorizationRequirements;

namespace XI.Portal.Auth.Extensions
{
    public static class PolicyExtensions
    {
        public static void AddXtremeIdiotsPolicies(this AuthorizationOptions options)
        {
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

            // Change Log
            options.AddPolicy(AuthPolicies.AccessChangeLog, policy => { policy.Requirements.Add(new AccessChangeLog()); });

            // Credentials
            options.AddPolicy(AuthPolicies.AccessCredentials, policy => { policy.Requirements.Add(new AccessCredentials()); });

            // Demos
            options.AddPolicy(AuthPolicies.AccessDemos, policy => { policy.Requirements.Add(new AccessDemos()); });
            options.AddPolicy(AuthPolicies.DeleteDemo, policy => { policy.Requirements.Add(new DeleteDemo()); });

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

            // Migration
            options.AddPolicy(AuthPolicies.AccessMigration, policy => { policy.Requirements.Add(new AccessMigration()); });

            // Players
            options.AddPolicy(AuthPolicies.AccessPlayers, policy => { policy.Requirements.Add(new AccessPlayers()); });
            options.AddPolicy(AuthPolicies.DeletePlayer, policy => { policy.Requirements.Add(new DeletePlayer()); });
            options.AddPolicy(AuthPolicies.ViewPlayers, policy => { policy.Requirements.Add(new ViewPlayers()); });

            // Server Admin
            options.AddPolicy(AuthPolicies.AccessLiveRcon, policy => { policy.Requirements.Add(new AccessLiveRcon()); });
            options.AddPolicy(AuthPolicies.AccessServerAdmin, policy => { policy.Requirements.Add(new AccessServerAdmin()); });
            options.AddPolicy(AuthPolicies.ViewGameChatLog, policy => { policy.Requirements.Add(new ViewGameChatLog()); });
            options.AddPolicy(AuthPolicies.ViewGlobalChatLog, policy => { policy.Requirements.Add(new ViewGlobalChatLog()); });
            options.AddPolicy(AuthPolicies.ViewLiveRcon, policy => { policy.Requirements.Add(new ViewLiveRcon()); });
            options.AddPolicy(AuthPolicies.ViewServerChatLog, policy => { policy.Requirements.Add(new ViewServerChatLog()); });

            // Servers
            options.AddPolicy(AuthPolicies.AccessServers, policy => { policy.Requirements.Add(new AccessServers()); });

            // Status
            options.AddPolicy(AuthPolicies.AccessStatus, policy => { policy.Requirements.Add(new AccessStatus()); });

            // Users
            options.AddPolicy(AuthPolicies.AccessUsers, policy => { policy.Requirements.Add(new AccessUsers()); });
            options.AddPolicy(AuthPolicies.CreateUserClaim, policy => { policy.Requirements.Add(new CreateUserClaim()); });
            options.AddPolicy(AuthPolicies.DeleteUserClaim, policy => { policy.Requirements.Add(new DeleteUserClaim()); });
        }
    }
}