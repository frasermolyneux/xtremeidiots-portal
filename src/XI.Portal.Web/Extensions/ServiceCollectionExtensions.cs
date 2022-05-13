using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using XI.Portal.Web.Auth.Handlers;
using XI.Portal.Web.Auth.XtremeIdiots;
using XI.Portal.Web.Configuration;
using XI.Portal.Web.Repository;

namespace XI.Portal.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddUsersModule(this IServiceCollection serviceCollection, Action<IUsersModuleOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            IUsersModuleOptions options = new UsersModuleOptions();
            configureOptions.Invoke(options);

            if (options.UsersRepositoryOptions != null)
            {
                IUsersRepositoryOptions subOptions = new UsersRepositoryOptions();
                options.UsersRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IUsersRepository, UsersRepository>();
            }
        }

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