using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddSingleton<IAuthorizationHandler, AdminActionsAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, BanFileMonitorsAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, ChangeLogAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, CredentialsAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, DemosAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, GameServersAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, HomeAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, MapsAuthHandler>();

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