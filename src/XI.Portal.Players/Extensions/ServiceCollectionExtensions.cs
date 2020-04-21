using System;
using FM.GeoLocation.Client;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Players.Configuration;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Repository;

namespace XI.Portal.Players.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPlayersModule(this IServiceCollection serviceCollection,
            Action<IPlayersModuleOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            IPlayersModuleOptions options = new PlayersModuleOptions();
            configureOptions.Invoke(options);

            options.Validate();

            if (options.PlayersRepositoryOptions != null)
            {
                IPlayersRepositoryOptions subOptions = new PlayersRepositoryOptions();
                options.PlayersRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IPlayersRepository, PlayersRepository>();
            }

            if (options.AdminActionsRepositoryOptions != null)
            {
                IAdminActionsRepositoryOptions subOptions = new AdminActionsRepositoryOptions();
                options.AdminActionsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IAdminActionsRepository, AdminActionsRepository>();
            }

            if (options.PlayerLocationsRepositoryOptions != null)
            {
                IPlayerLocationsRepositoryOptions subOptions = new PlayerLocationsRepositoryOptions();
                options.PlayerLocationsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IPlayerLocationsRepository, PlayerLocationsRepository>();

                serviceCollection.AddSingleton<IGeoLocationClientConfiguration>(subOptions.GeoLocationClientConfiguration);
                serviceCollection.AddSingleton<IGeoLocationClient, GeoLocationClient>();
            }
        }
    }
}