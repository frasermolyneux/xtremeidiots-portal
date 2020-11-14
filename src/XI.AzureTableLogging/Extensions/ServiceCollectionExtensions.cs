using Microsoft.Extensions.DependencyInjection;
using System;
using XI.AzureTableLogging.Configuration;
using XI.AzureTableLogging.Interfaces;
using XI.AzureTableLogging.Logger;

namespace XI.AzureTableLogging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDirectAzureTableLogger(this IServiceCollection serviceCollection, Action<IAzureTableLoggerOptions> configureOptions)
        {
            IAzureTableLoggerOptions options = new AzureTableLoggerOptions();
            configureOptions.Invoke(options);

            serviceCollection.AddSingleton(options);
            serviceCollection.AddScoped<AzureTableLogger>();
        }
    }
}
