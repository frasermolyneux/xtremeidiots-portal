using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Maps.Configuration;
using XI.Portal.Maps.Repository;

namespace XI.Portal.Maps.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMapsRepository(this IServiceCollection serviceCollection,
            Action<IMapsOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var options = new MapsOptions();
            configureOptions.Invoke(options);

            options.Validate();

            serviceCollection.AddSingleton<IMapsOptions>(options);
            serviceCollection.AddScoped<IMapsRepository, MapsRepository>();
        }
    }
}