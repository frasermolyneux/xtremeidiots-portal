using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Auth.AdminActions.AuthorizationHandlers;
using XI.Portal.Auth.BanFileMonitors.AuthorizationHandlers;
using XI.Portal.Auth.Credentials.AuthorizationHandlers;
using XI.Portal.Auth.FileMonitors.AuthorizationHandlers;
using XI.Portal.Auth.GameServers.AuthorizationHandlers;
using XI.Portal.Auth.RconMonitors.AuthorizationHandlers;
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

            // Rcon Monitors
            services.AddSingleton<IAuthorizationHandler, AccessRconMonitorsHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateRconMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteRconMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, EditRconMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewRconMonitorHandler>();
        }
    }
}