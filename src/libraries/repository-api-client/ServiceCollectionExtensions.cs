using Microsoft.Extensions.DependencyInjection;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApiClient.Api;

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
            Action<RepositoryApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IRepositoryApiTokenProvider, RepositoryApiTokenProvider>();

            serviceCollection.AddSingleton<IAdminActionsApi, AdminActionsApi>();
            serviceCollection.AddSingleton<IBanFileMonitorsApi, BanFileMonitorsApi>();
            serviceCollection.AddSingleton<IChatMessagesApi, ChatMessagesApi>();
            serviceCollection.AddSingleton<IDataMaintenanceApi, DataMaintenanceApi>();
            serviceCollection.AddSingleton<IDemosAuthApi, DemosAuthApi>();
            serviceCollection.AddSingleton<IDemosApi, DemosApi>();
            serviceCollection.AddSingleton<IGameServersApi, GameServersApi>();
            serviceCollection.AddSingleton<IGameServersEventsApi, GameServersEventsApi>();
            serviceCollection.AddSingleton<IGameServersStatsApi, GameServersStatsApi>();
            serviceCollection.AddSingleton<ILivePlayersApi, LivePlayersApi>();
            serviceCollection.AddSingleton<IMapsApi, MapsApi>();
            serviceCollection.AddSingleton<IPlayerAnalyticsApi, PlayerAnalyticsApi>();
            serviceCollection.AddSingleton<IPlayersApi, PlayersApi>();
            serviceCollection.AddSingleton<IRecentPlayersApi, RecentPlayersApi>();
            serviceCollection.AddSingleton<IReportsApi, ReportsApi>();
            serviceCollection.AddSingleton<IUserProfileApi, UserProfileApi>();

            serviceCollection.AddSingleton<IRepositoryApiClient, RepositoryApiClient>();
        }
    }
}