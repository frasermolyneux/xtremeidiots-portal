using Microsoft.Extensions.DependencyInjection;
using System;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.AdminActionsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayerAnalyticsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
            Action<RepositoryApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IRepositoryTokenProvider, RepositoryTokenProvider>();

            serviceCollection.AddSingleton<IAdminActionsApiClient, AdminActionsApiClient>();
            serviceCollection.AddSingleton<IBanFileMonitorsApiClient, BanFileMonitorsApiClient>();
            serviceCollection.AddSingleton<IChatMessagesApiClient, ChatMessagesApiClient>();
            serviceCollection.AddSingleton<IDemosApiClient, DemosApiClient>();
            serviceCollection.AddSingleton<IGameServersApiClient, GameServersApiClient>();
            serviceCollection.AddSingleton<IPlayerAnalyticsApiClient, PlayerAnalyticsApiClient>();
            serviceCollection.AddSingleton<IPlayersApiClient, PlayersApiClient>();

            serviceCollection.AddSingleton<IRepositoryApiClient, RepositoryApiClient>();
        }
    }
}