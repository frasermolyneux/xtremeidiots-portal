using Microsoft.Extensions.DependencyInjection;

namespace XtremeIdiots.Portal.SyncFunc.Redirect
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMapRedirectRepository(this IServiceCollection serviceCollection, Action<MapRedirectRepositoryOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            serviceCollection.Configure(configureOptions);

            serviceCollection.AddSingleton(configureOptions);
            serviceCollection.AddScoped<IMapRedirectRepository, MapRedirectRepository>();

        }
    }
}