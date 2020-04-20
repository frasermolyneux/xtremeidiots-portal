using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Maps.Configuration;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Maps.Repository;

namespace XI.Portal.Maps.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMapsModule(this IServiceCollection serviceCollection,
            Action<IMapsModuleOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            IMapsModuleOptions options = new MapsModuleOptions();
            configureOptions.Invoke(options);

            options.Validate();

            if (options.MapFileRepositoryOptions != null)
            {
                IMapFileRepositoryOptions supOptions = new MapFileRepositoryOptions();
                options.MapFileRepositoryOptions.Invoke(supOptions);

                supOptions.Validate();

                serviceCollection.AddSingleton(supOptions);
                serviceCollection.AddScoped<IMapFileRepository, MapFileRepository>();
            }

            if (options.MapImageRepositoryOptions != null)
            {
                IMapImageRepositoryOptions subOptions = new MapImageRepositoryOptions();
                options.MapImageRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IMapImageRepository, MapImageRepository>();
            }

            if (options.MapsRepositoryOptions != null)
            {
                IMapsRepositoryOptions subOptions = new MapsRepositoryOptions();
                options.MapsRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IMapsRepository, MapsRepository>();
            }
        }
    }
}