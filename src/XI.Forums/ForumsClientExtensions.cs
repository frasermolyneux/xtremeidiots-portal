using System;
using Microsoft.Extensions.DependencyInjection;

namespace XI.Forums
{
    public static class ForumsClientExtensions
    {
        public static void AddForumsClient(this IServiceCollection serviceCollection,
            Action<ForumsOptions> configureOptions)
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