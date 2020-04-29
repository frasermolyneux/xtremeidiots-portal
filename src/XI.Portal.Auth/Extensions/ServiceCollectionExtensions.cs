using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Auth.AdminActions.AuthorizationHandlers;
using XI.Portal.Auth.BanFileMonitors.AuthorizationHandlers;
using XI.Portal.Auth.XtremeIdiots;

namespace XI.Portal.Auth.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddXtremeIdiotsAuth(this IServiceCollection services)
        {
            services.AddScoped<IXtremeIdiotsAuth, XtremeIdiotsAuth>();

            // Admin Actions
            services.AddSingleton<IAuthorizationHandler, AccessAdminActionsControllerHandler>();
            services.AddSingleton<IAuthorizationHandler, ChangeAdminActionAdminHandler>();
            services.AddSingleton<IAuthorizationHandler, ClaimAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateAdminActionTopicHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, EditAdminActionHandler>();
            services.AddSingleton<IAuthorizationHandler, LiftAdminActionHandler>();

            // Ban File Monitors
            services.AddSingleton<IAuthorizationHandler, CreateBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, EditBanFileMonitorHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewBanFileMonitorHandler>();
        }
    }
}