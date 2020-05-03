using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Helpers;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Repository;
using XI.Servers.Factories;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServersModule(this IServiceCollection serviceCollection,
            Action<IServersModuleOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            IServersModuleOptions options = new ServersModuleOptions();
            configureOptions.Invoke(options);

            if (options.GameServerStatusRepositoryOptions != null)
            {
                IGameServerStatusRepositoryOptions subOptions = new GameServerStatusRepositoryOptions();
                options.GameServerStatusRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IGameServerStatusRepository, GameServerStatusRepository>();
            }

            if (options.GameServerStatusStatsRepositoryOptions != null)
            {
                IGameServerStatusStatsRepositoryOptions subOptions = new GameServerStatusStatsRepositoryOptions();
                options.GameServerStatusStatsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IGameServerStatusStatsRepository, GameServerStatusStatsRepository>();
            }

            serviceCollection.AddScoped<IGameServersRepository, GameServersRepository>();
            serviceCollection.AddScoped<IBanFileMonitorsRepository, BanFileMonitorsRepository>();
            serviceCollection.AddScoped<IFileMonitorsRepository, FileMonitorsRepository>();
            serviceCollection.AddScoped<IRconMonitorsRepository, RconMonitorsRepository>();
            serviceCollection.AddScoped<IChatLogsRepository, ChatLogsRepository>();

            serviceCollection.AddSingleton<IFtpHelper, FtpHelper>();
            serviceCollection.AddSingleton<IQueryClientFactory, QueryClientFactory>();
            serviceCollection.AddSingleton<IRconClientFactory, RconClientFactory>();
            serviceCollection.AddSingleton<IGameServerClientFactory, GameServerClientFactory>();
        }
    }
}