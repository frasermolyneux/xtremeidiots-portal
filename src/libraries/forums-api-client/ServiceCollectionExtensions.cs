using Microsoft.Extensions.DependencyInjection;

namespace XtremeIdiots.Portal.ForumsApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
            Action<ForumsApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);


            serviceCollection.AddSingleton<IForumsApiClient, ForumsApiClient>();
        }
    }
}