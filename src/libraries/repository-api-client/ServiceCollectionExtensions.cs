using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.RepositoryApiClient.AdminActionsApi;
using XtremeIdiots.Portal.RepositoryApiClient.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;
using XtremeIdiots.Portal.RepositoryApiClient.DemosRepositoryApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;
using XtremeIdiots.Portal.RepositoryApiClient.MapsApi;
using XtremeIdiots.Portal.RepositoryApiClient.PlayerAnalyticsApi;
using XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
            Action<RepositoryApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IAdminActionsApiClient, AdminActionsApiClient>();
            serviceCollection.AddSingleton<IBanFileMonitorsApiClient, BanFileMonitorsApiClient>();
            serviceCollection.AddSingleton<IChatMessagesApiClient, ChatMessagesApiClient>();
            serviceCollection.AddSingleton<IDataMaintenanceApiClient, DataMaintenanceApiClient>();
            serviceCollection.AddSingleton<IDemosApiClient, DemosApiClient>();
            serviceCollection.AddSingleton<IGameServersApiClient, GameServersApiClient>();
            serviceCollection.AddSingleton<IGameServersEventsApiClient, GameServersEventsApiClient>();
            serviceCollection.AddSingleton<IMapsApiClient, MapsApiClient>();
            serviceCollection.AddSingleton<IPlayerAnalyticsApiClient, PlayerAnalyticsApiClient>();
            serviceCollection.AddSingleton<IPlayersApiClient, PlayersApiClient>();

            serviceCollection.AddSingleton<IRepositoryApiClient, RepositoryApiClient>();
        }
    }
}