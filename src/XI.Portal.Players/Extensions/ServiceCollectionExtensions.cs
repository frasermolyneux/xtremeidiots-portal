using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Players.Configuration;
using XI.Portal.Players.Forums;
using XI.Portal.Players.Ingest;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Repository;
using XI.Portal.Players.Validators;

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

            if (options.BanFilesRepositoryOptions != null)
            {
                IBanFilesRepositoryOptions subOptions = new BanFilesRepositoryOptions();
                options.BanFilesRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IBanFilesRepository, BanFilesRepository>();
            }

            serviceCollection.AddScoped<IBanFileIngest, BanFileIngest>();
            serviceCollection.AddScoped<IGuidValidator, GuidValidator>();

            serviceCollection.AddSingleton<IPlayersForumsClient, PlayersForumsClient>();
        }
    }
}