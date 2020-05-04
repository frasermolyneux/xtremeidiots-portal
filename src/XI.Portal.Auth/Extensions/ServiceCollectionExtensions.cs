using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Auth.AdminActions.AuthorizationHandlers;
using XI.Portal.Auth.BanFileMonitors.AuthorizationHandlers;
using XI.Portal.Auth.Credentials.AuthorizationHandlers;
using XI.Portal.Auth.Demos.AuthorizationHandlers;
using XI.Portal.Auth.FileMonitors.AuthorizationHandlers;
using XI.Portal.Auth.GameServers.AuthorizationHandlers;
using XI.Portal.Auth.Home.AuthorizationHandlers;
using XI.Portal.Auth.Maps.AuthorizationHandlers;
using XI.Portal.Auth.Migration.AuthorizationHandlers;
using XI.Portal.Auth.Players.AuthorizationHandlers;
using XI.Portal.Auth.ServerAdmin.AuthorizationHandlers;
using XI.Portal.Auth.Servers.AuthorizationHandlers;
using XI.Portal.Auth.Status.AuthorizationHandlers;
using XI.Portal.Auth.Users.AuthorizationHandlers;
using XI.Portal.Auth.XtremeIdiots;

namespace XI.Portal.Auth.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddXtremeIdiotsAuth(this IServiceCollection services)
        {
            services.AddScoped<IXtremeIdiotsAuth, XtremeIdiotsAuth>();

            // Admin Actions
            services.AddSingleton<IAuthorizationHandler, AccessAdminActionsHandler>();
            services.AddSingleton<IAuthorizationHandler, ChangeAdminActionAdminHandler>();
            services.AddSingleton<IAuthorizationHandler, ClaimAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateAdminActionTopicHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, EditAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, LiftAdminActionHandler>();

            // Ban File Monitors
            services.AddSingleton<IAuthorizationHandler, AccessBanFileMonitorsHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, EditBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewBanFileMonitorHandler>();

            // Credentials
            services.AddSingleton<IAuthorizationHandler, AccessCredentialsHandler>();

            // Demos
            services.AddSingleton<IAuthorizationHandler, AccessDemosHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteDemoHandler>();

            // File Monitors
            services.AddSingleton<IAuthorizationHandler, AccessFileMonitorsHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, EditFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewFileMonitorHandler>();

            // Game Servers
            services.AddSingleton<IAuthorizationHandler, AccessGameServersHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateGameServerHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteGameServerHandler>();
            services.AddSingleton<IAuthorizationHandler, EditGameServerFtpHandler>();
            services.AddSingleton<IAuthorizationHandler, EditGameServerHandler>();
            services.AddSingleton<IAuthorizationHandler, EditGameServerRconHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewFtpCredentialHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewGameServerHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewRconCredentialHandler>();

            // Home
            services.AddSingleton<IAuthorizationHandler, AccessHomeHandler>();

            // Maps
            services.AddSingleton<IAuthorizationHandler, AccessMapsHandler>();

            // Migration
            services.AddSingleton<IAuthorizationHandler, AccessMigrationHandler>();

            // Players
            services.AddSingleton<IAuthorizationHandler, AccessPlayersHandler>();
            services.AddSingleton<IAuthorizationHandler, DeletePlayerHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewPlayersHandler>();

            // Server Admin
            services.AddSingleton<IAuthorizationHandler, AccessLiveRconHandler>();
            services.AddSingleton<IAuthorizationHandler, AccessServerAdminHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewGameChatLogHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewGlobalChatLogHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewLiveRconHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewServerChatLogHandler>();

            // Servers
            services.AddSingleton<IAuthorizationHandler, AccessServersHandler>();

            // Status
            services.AddSingleton<IAuthorizationHandler, AccessStatusHandler>();

            // Users
            services.AddSingleton<IAuthorizationHandler, AccessUsersHandler>();
        }
    }
}