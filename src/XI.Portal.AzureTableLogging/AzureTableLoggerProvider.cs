using System;
using Microsoft.Extensions.Logging;

namespace XI.Portal.AzureTableLogging
{
    public class AzureTableLoggerProvider : ILoggerProvider
    {
        private readonly Action<AzureTableLoggerOptions> _configureOptions;

        public AzureTableLoggerProvider(Action<AzureTableLoggerOptions> configureOptions)
        {
            _configureOptions = configureOptions;
        }

        public ILogger CreateLogger(string categoryName)
        {
            var options = new AzureTableLoggerOptions();
            _configureOptions.Invoke(options);

            options.Validate();

            return new AzureTableLogger(options);
        }

        public void Dispose()
        {
        }
    }
}