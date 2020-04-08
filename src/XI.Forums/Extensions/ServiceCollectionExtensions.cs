using System;
using Microsoft.Extensions.DependencyInjection;
using XI.Forums.Client;
using XI.Forums.Configuration;

namespace XI.Forums.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddForumsClient(this IServiceCollection serviceCollection,
            Action<IForumsOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var options = new ForumsOptions();
            configureOptions.Invoke(options);

            options.Validate();

            serviceCollection.AddSingleton<IForumsOptions>(options);
            serviceCollection.AddSingleton<IForumsClient, ForumsClient>();
        }
    }
}