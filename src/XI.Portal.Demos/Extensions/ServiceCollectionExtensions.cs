using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Demos.Configuration;
using XI.Portal.Demos.Forums;
using XI.Portal.Demos.Interfaces;
using XI.Portal.Demos.Repository;

namespace XI.Portal.Demos.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDemosModule(this IServiceCollection serviceCollection,
            Action<IDemoModuleOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            IDemoModuleOptions options = new DemoModuleOptions();
            configureOptions.Invoke(options);

            options.Validate();

            if (options.DemoAuthRepositoryOptions != null)
            {
                IDemoAuthRepositoryOptions subOptions = new DemoAuthRepositoryOptions();
                options.DemoAuthRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IDemoAuthRepository, DemoAuthRepository>();
            }

            if (options.DemosRepositoryOptions != null)
            {
                IDemosRepositoryOptions subOptions = new DemosRepositoryOptions();
                options.DemosRepositoryOptions.Invoke(subOptions);

                subOptions.Validate();

                serviceCollection.AddSingleton(subOptions);
                serviceCollection.AddScoped<IDemosRepository, DemosRepository>();
            }

            serviceCollection.AddScoped<IDemosForumsClient, DemosForumsClient>();
        }
    }
}