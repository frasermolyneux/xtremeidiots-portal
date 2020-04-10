using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Players.Configuration;
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
                IPlayersRepositoryOptions playersRepositoryOptions = new PlayersRepositoryOptions();
                options.PlayersRepositoryOptions.Invoke(playersRepositoryOptions);

                playersRepositoryOptions.Validate();

                serviceCollection.AddSingleton(playersRepositoryOptions);
                serviceCollection.AddScoped<IPlayersRepository, PlayersRepository>();
            }
        }
    }
}