using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.RepositoryApiClient.AdminActionsApi;
using XtremeIdiots.Portal.RepositoryApiClient.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;
using XtremeIdiots.Portal.RepositoryApiClient.DemosAuthApi;
using XtremeIdiots.Portal.RepositoryApiClient.DemosRepositoryApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersStatsApi;
using XtremeIdiots.Portal.RepositoryApiClient.LivePlayersApi;
using XtremeIdiots.Portal.RepositoryApiClient.MapsApi;
using XtremeIdiots.Portal.RepositoryApiClient.PlayerAnalyticsApi;
using XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;
using XtremeIdiots.Portal.RepositoryApiClient.RecentPlayersApi;
using XtremeIdiots.Portal.RepositoryApiClient.ReportsApi;
using XtremeIdiots.Portal.RepositoryApiClient.UserProfileApi;

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
            Action<RepositoryApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IRepositoryApiTokenProvider, RepositoryApiTokenProvider>();

            serviceCollection.AddSingleton<IAdminActionsApiClient, AdminActionsApiClient>();
            serviceCollection.AddSingleton<IBanFileMonitorsApiClient, BanFileMonitorsApiClient>();
            serviceCollection.AddSingleton<IChatMessagesApiClient, ChatMessagesApiClient>();
            serviceCollection.AddSingleton<IDataMaintenanceApiClient, DataMaintenanceApiClient>();
            serviceCollection.AddSingleton<IDemosAuthApiClient, DemosAuthApiClient>();
            serviceCollection.AddSingleton<IDemosApiClient, DemosApiClient>();
            serviceCollection.AddSingleton<IGameServersApiClient, GameServersApiClient>();
            serviceCollection.AddSingleton<IGameServersEventsApiClient, GameServersEventsApiClient>();
            serviceCollection.AddSingleton<IGameServersStatsApiClient, GameServersStatsApiClient>();
            serviceCollection.AddSingleton<ILivePlayersApiClient, LivePlayersApiClient>();
            serviceCollection.AddSingleton<IMapsApiClient, MapsApiClient>();
            serviceCollection.AddSingleton<IPlayerAnalyticsApiClient, PlayerAnalyticsApiClient>();
            serviceCollection.AddSingleton<IPlayersApiClient, PlayersApiClient>();
            serviceCollection.AddSingleton<IRecentPlayersApiClient, RecentPlayersApiClient>();
            serviceCollection.AddSingleton<IReportsApiClient, ReportsApiClient>();
            serviceCollection.AddSingleton<IUserProfileApiClient, UserProfileApiClient>();

            serviceCollection.AddSingleton<IRepositoryApiClient, RepositoryApiClient>();
        }
    }
}