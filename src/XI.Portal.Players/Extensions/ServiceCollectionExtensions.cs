using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Players.Configuration;
using XI.Portal.Players.Forums;
using XI.Portal.Players.Ingest;
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

            if (options.PlayerLocationsRepositoryOptions != null)
            {
                IPlayerLocationsRepositoryOptions subOptions = new PlayerLocationsRepositoryOptions();
                options.PlayerLocationsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IPlayerLocationsRepository, PlayerLocationsRepository>();
            }

            if (options.PlayersCacheRepositoryOptions != null)
            {
                IPlayersCacheRepositoryOptions subOptions = new PlayersCacheRepositoryOptions();
                options.PlayersCacheRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IPlayersCacheRepository, PlayersCacheRepository>();
            }

            serviceCollection.AddScoped<IPlayersRepository, PlayersRepository>();
            serviceCollection.AddScoped<IAdminActionsRepository, AdminActionsRepository>();
            serviceCollection.AddScoped<IPlayerIngest, PlayerIngest>();

            serviceCollection.AddSingleton<IPlayersForumsClient, PlayersForumsClient>();
        }
    }
}