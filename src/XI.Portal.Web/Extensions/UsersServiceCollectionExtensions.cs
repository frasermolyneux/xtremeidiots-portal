using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Web.Configuration;
using XI.Portal.Web.Repository;

namespace XI.Portal.Web.Extensions
{
    public static class UsersServiceCollectionExtensions
    {
        public static void AddUsersModule(this IServiceCollection serviceCollection,
            Action<IUsersModuleOptions> configureOptions)
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
    }
}