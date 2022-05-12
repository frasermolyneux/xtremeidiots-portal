using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Auth.BanFileMonitors.AuthorizationHandlers;
using XI.Portal.Auth.ChangeLog.AuthorizationHandlers;
using XI.Portal.Auth.Credentials.AuthorizationHandlers;
using XI.Portal.Auth.Demos.AuthorizationHandlers;
using XI.Portal.Auth.Home.AuthorizationHandlers;
using XI.Portal.Auth.Maps.AuthorizationHandlers;
using XI.Portal.Auth.Migration.AuthorizationHandlers;
using XI.Portal.Auth.Players.AuthorizationHandlers;
using XI.Portal.Auth.ServerAdmin.AuthorizationHandlers;
using XI.Portal.Auth.Servers.AuthorizationHandlers;
using XI.Portal.Auth.Status.AuthorizationHandlers;
using XI.Portal.Auth.Users.AuthorizationHandlers;
using XI.Portal.Auth.XtremeIdiots;
using XI.Portal.Web.Auth;

namespace XI.Portal.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddXtremeIdiotsAuth(this IServiceCollection services)
        {
            services.AddScoped<IXtremeIdiotsAuth, XtremeIdiotsAuth>();

            // Admin Actions
            services.AddSingleton<IAuthorizationHandler, AdminActionsAuthHandler>();

            // Ban File Monitors
            services.AddSingleton<IAuthorizationHandler, AccessBanFileMonitorsHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, EditBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewBanFileMonitorHandler>();

            // Change Log
            services.AddSingleton<IAuthorizationHandler, AccessChangeLogHandler>();

            // Credentials
            services.AddSingleton<IAuthorizationHandler, AccessCredentialsHandler>();

            // Demos
            services.AddSingleton<IAuthorizationHandler, AccessDemosHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteDemoHandler>();

            // Game Servers
            services.AddSingleton<IAuthorizationHandler, GameServersAuthHandler>();

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
            services.AddSingleton<IAuthorizationHandler, CreateUserClaimHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteUserClaimHandler>();
        }
    }
}