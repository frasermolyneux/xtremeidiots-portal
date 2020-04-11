using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Users.Configuration;
using XI.Portal.Users.Repository;

namespace XI.Portal.Users.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPlayersModule(this IServiceCollection serviceCollection,
            Action<IUsersModuleOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            IUsersModuleOptions options = new UsersModuleOptions();
            configureOptions.Invoke(options);

            options.Validate();

            if (options.UsersRepositoryOptions != null)
            {
                IUsersRepositoryOptions playersRepositoryOptions = new UsersRepositoryOptions();
                options.UsersRepositoryOptions.Invoke(playersRepositoryOptions);

                playersRepositoryOptions.Validate();

                serviceCollection.AddSingleton(playersRepositoryOptions);
                serviceCollection.AddScoped<IUsersRepository, UsersRepository>();
            }
        }
    }
}