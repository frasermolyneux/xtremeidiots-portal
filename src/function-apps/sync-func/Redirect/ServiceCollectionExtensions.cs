using Microsoft.Extensions.DependencyInjection;
using System;

namespace XtremeIdiots.Portal.SyncFunc.Redirect
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMapRedirectRepository(this IServiceCollection serviceCollection, Action<IMapRedirectRepositoryOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            serviceCollection.AddSingleton(configureOptions);
            serviceCollection.AddScoped<IMapRedirectRepository, MapRedirectRepository>();

        }
    }
}