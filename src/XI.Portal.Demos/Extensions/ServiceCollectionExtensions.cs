using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Portal.Demos.Configuration;
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
                IDemoAuthRepositoryOptions demoAuthRepositoryOptions = new DemoAuthRepositoryOptions();
                options.DemoAuthRepositoryOptions.Invoke(demoAuthRepositoryOptions);

                demoAuthRepositoryOptions.Validate();

                serviceCollection.AddSingleton(demoAuthRepositoryOptions);
                serviceCollection.AddScoped<IDemoAuthRepository, DemoAuthRepository>();
            }

            if (options.DemosRepositoryOptions != null)
            {
                IDemosRepositoryOptions demosRepositoryOptions = new DemosRepositoryOptions();
                options.DemosRepositoryOptions.Invoke(demosRepositoryOptions);

                demosRepositoryOptions.Validate();

                serviceCollection.AddSingleton(demosRepositoryOptions);
                serviceCollection.AddScoped<IDemosRepository, DemosRepository>();
            }
        }
    }
}