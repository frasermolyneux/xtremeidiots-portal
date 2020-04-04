using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace XI.AzureTableLogging
{
    public static class AzureTableLoggerExtensions
    {
        public static ILoggingBuilder AddAzureTableLogger(this ILoggingBuilder loggingBuilder, Action<AzureTableLoggerOptions> configureOptions)
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