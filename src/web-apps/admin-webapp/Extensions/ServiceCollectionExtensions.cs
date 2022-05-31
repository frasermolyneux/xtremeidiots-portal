using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.AdminWebApp.Auth.Handlers;
using XtremeIdiots.Portal.AdminWebApp.Auth.XtremeIdiots;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
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
            services.AddSingleton<IAuthorizationHandler, PlayersAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, ServerAdminAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, ServersAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, StatusAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, UsersAuthHandler>();
        }
    }
}