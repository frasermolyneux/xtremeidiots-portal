using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Helpers;
using XI.Portal.Servers.Repository;
using XI.Servers.Factories;
using XI.Servers.Query.Factories;
using XI.Servers.Rcon.Factories;

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

            options.Validate();

            if (options.GameServersRepositoryOptions != null)
            {
                IGameServersRepositoryOptions subOptions = new GameServersRepositoryOptions();
                options.GameServersRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IGameServersRepository, GameServersRepository>();
            }

            if (options.BanFileMonitorsRepositoryOptions != null)
            {
                IBanFileMonitorsRepositoryOptions subOptions = new BanFileMonitorsRepositoryOptions();
                options.BanFileMonitorsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IBanFileMonitorsRepository, BanFileMonitorsRepository>();
            }

            if (options.FileMonitorsRepositoryOptions != null)
            {
                IFileMonitorsRepositoryOptions subOptions = new FileMonitorsRepositoryOptions();
                options.FileMonitorsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IFileMonitorsRepository, FileMonitorsRepository>();
            }

            if (options.RconMonitorsRepositoryOptions != null)
            {
                IRconMonitorsRepositoryOptions subOptions = new RconMonitorsRepositoryOptions();
                options.RconMonitorsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IRconMonitorsRepository, RconMonitorsRepository>();
            }

            serviceCollection.AddSingleton<IFtpHelper, FtpHelper>();
            serviceCollection.AddSingleton<IQueryClientFactory, QueryClientFactory>();
            serviceCollection.AddSingleton<IRconClientFactory, RconClientFactory>();
            serviceCollection.AddSingleton<IGameServerStatusHelperFactory, GameServerStatusHelperFactory>();
        }
    }
}