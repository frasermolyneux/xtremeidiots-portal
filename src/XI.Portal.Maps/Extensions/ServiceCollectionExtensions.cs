﻿using Microsoft.Extensions.DependencyInjection;
using System;
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

            if (options.MapImageRepositoryOptions != null)
            {
                IMapImageRepositoryOptions subOptions = new MapImageRepositoryOptions();
                options.MapImageRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IMapImageRepository, MapImageRepository>();
            }

            if (options.MapRedirectRepositoryOptions != null)
            {
                IMapRedirectRepositoryOptions subOptions = new MapRedirectRepositoryOptions();
                options.MapRedirectRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IMapRedirectRepository, MapRedirectRepository>();
            }
        }
    }
}