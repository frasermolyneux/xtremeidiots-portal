using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Maps.Configuration;
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
                IMapFileRepositoryOptions mapFileRepositoryOptions = new MapFileRepositoryOptions();
                options.MapFileRepositoryOptions.Invoke(mapFileRepositoryOptions);

                mapFileRepositoryOptions.Validate();

                serviceCollection.AddSingleton(mapFileRepositoryOptions);
                serviceCollection.AddScoped<IMapFileRepository, MapFileRepository>();
            }

            if (options.MapImageRepositoryOptions != null)
            {
                IMapImageRepositoryOptions mapImageRepositoryOptions = new MapImageRepositoryOptions();
                options.MapImageRepositoryOptions.Invoke(mapImageRepositoryOptions);

                mapImageRepositoryOptions.Validate();

                serviceCollection.AddSingleton(mapImageRepositoryOptions);
                serviceCollection.AddScoped<IMapImageRepository, MapImageRepository>();
            }

            if (options.MapsRepositoryOptions != null)
            {
                IMapsRepositoryOptions mapsRepositoryOptions = new MapsRepositoryOptions();
                options.MapsRepositoryOptions.Invoke(mapsRepositoryOptions);

                mapsRepositoryOptions.Validate();

                serviceCollection.AddSingleton(mapsRepositoryOptions);
                serviceCollection.AddScoped<IMapsRepository, MapsRepository>();
            }
        }
    }
}