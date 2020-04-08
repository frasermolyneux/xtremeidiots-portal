using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XI.AzureTableLogging.Configuration;
using XI.AzureTableLogging.Logger;

namespace XI.AzureTableLogging.Extensions
{
    public static class AzureTableLoggerExtensions
    {
        public static ILoggingBuilder AddAzureTableLogger(this ILoggingBuilder loggingBuilder, Action<IAzureTableLoggerOptions> configureOptions)
        {
            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            loggingBuilder.Services.AddLogging(builder =>
                builder.AddProvider(new AzureTableLoggerProvider(configureOptions)));

            return loggingBuilder;
        }
    }
}