using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.ServersApiClient.QueryApi;
using XtremeIdiots.Portal.ServersApiClient.RconApi;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServersApiClient(this IServiceCollection serviceCollection,
            Action<ServersApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IServersApiTokenProvider, ServersApiTokenProvider>();

            serviceCollection.AddSingleton<IQueryApiClient, QueryApiClient>();
            serviceCollection.AddSingleton<IRconApiClient, RconApiClient>();

            serviceCollection.AddSingleton<IServersApiClient, ServersApiClient>();
        }
    }
}