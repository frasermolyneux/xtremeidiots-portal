using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Extensions
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

            // Profile
            options.AddPolicy(AuthPolicies.AccessProfile, policy => { policy.Requirements.Add(new AccessProfile()); });

            // Maps
            options.AddPolicy(AuthPolicies.AccessMaps, policy => { policy.Requirements.Add(new AccessMaps()); });
            options.AddPolicy(AuthPolicies.AccessMapManagerController, policy => { policy.Requirements.Add(new AccessMapManagerController()); });
            options.AddPolicy(AuthPolicies.ManageMaps, policy => { policy.Requirements.Add(new ManageMaps()); });
            options.AddPolicy(AuthPolicies.CreateMapPack, policy => { policy.Requirements.Add(new CreateMapPack()); });
            options.AddPolicy(AuthPolicies.EditMapPack, policy => { policy.Requirements.Add(new EditMapPack()); });
            options.AddPolicy(AuthPolicies.DeleteMapPack, policy => { policy.Requirements.Add(new DeleteMapPack()); });
            options.AddPolicy(AuthPolicies.PushMapToRemote, policy => { policy.Requirements.Add(new PushMapToRemote()); });
            options.AddPolicy(AuthPolicies.DeleteMapFromHost, policy => { policy.Requirements.Add(new DeleteMapFromHost()); });

            // Players
            options.AddPolicy(AuthPolicies.AccessPlayers, policy => { policy.Requirements.Add(new AccessPlayers()); });
            options.AddPolicy(AuthPolicies.DeletePlayer, policy => { policy.Requirements.Add(new DeletePlayer()); });
            options.AddPolicy(AuthPolicies.ViewPlayers, policy => { policy.Requirements.Add(new ViewPlayers()); });
            options.AddPolicy(AuthPolicies.CreateProtectedName, policy => { policy.Requirements.Add(new CreateProtectedName()); });
            options.AddPolicy(AuthPolicies.DeleteProtectedName, policy => { policy.Requirements.Add(new DeleteProtectedName()); });
            options.AddPolicy(AuthPolicies.ViewProtectedName, policy => { policy.Requirements.Add(new ViewProtectedName()); });

            // Player Tags
            options.AddPolicy(AuthPolicies.AccessPlayerTags, policy => { policy.Requirements.Add(new AccessPlayerTags()); });
            options.AddPolicy(AuthPolicies.CreatePlayerTag, policy => { policy.Requirements.Add(new CreatePlayerTag()); });
            options.AddPolicy(AuthPolicies.EditPlayerTag, policy => { policy.Requirements.Add(new EditPlayerTag()); });
            options.AddPolicy(AuthPolicies.DeletePlayerTag, policy => { policy.Requirements.Add(new DeletePlayerTag()); });

            // Server Admin
            options.AddPolicy(AuthPolicies.AccessLiveRcon, policy => { policy.Requirements.Add(new AccessLiveRcon()); });
            options.AddPolicy(AuthPolicies.AccessServerAdmin, policy => { policy.Requirements.Add(new AccessServerAdmin()); });
            options.AddPolicy(AuthPolicies.ViewGameChatLog, policy => { policy.Requirements.Add(new ViewGameChatLog()); });
            options.AddPolicy(AuthPolicies.ViewGlobalChatLog, policy => { policy.Requirements.Add(new ViewGlobalChatLog()); });
            options.AddPolicy(AuthPolicies.ViewLiveRcon, policy => { policy.Requirements.Add(new ViewLiveRcon()); });
            options.AddPolicy(AuthPolicies.ViewServerChatLog, policy => { policy.Requirements.Add(new ViewServerChatLog()); });
            options.AddPolicy(AuthPolicies.ManageMaps, policy => { policy.Requirements.Add(new ManageMaps()); });
            options.AddPolicy(AuthPolicies.LockChatMessages, policy => { policy.Requirements.Add(new LockChatMessages()); });

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