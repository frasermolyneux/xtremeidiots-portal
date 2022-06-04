using Microsoft.Extensions.DependencyInjection;

using XtremeIdiots.Portal.ServersApiClient.Api;
using XtremeIdiots.Portal.ServersApiClient.Interfaces;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServersApiClient(this IServiceCollection serviceCollection,
            Action<ServersApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IServersApiTokenProvider, ServersApiTokenProvider>();

            serviceCollection.AddSingleton<IQueryApi, QueryApi>();
            serviceCollection.AddSingleton<IRconApi, RconApi>();

            serviceCollection.AddSingleton<IServersApiClient, ServersApiClient>();
        }
    }
}