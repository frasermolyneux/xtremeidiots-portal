using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.AdminActionsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayerAnalyticsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public class RepositoryApiClient : IRepositoryApiClient
    {
        public RepositoryApiClient(
            IAdminActionsApiClient adminActionsApiClient,
            IBanFileMonitorsApiClient banFileMonitorsApiClient,
            IChatMessagesApiClient chatMessagesApiClient,
            IDemosApiClient demosApiClient,
            IGameServersApiClient gameServersApiClient,
            IPlayerAnalyticsApiClient playerAnalyticsApiClient,
            IPlayersApiClient playersApiClient)
        {
            AdminActions = adminActionsApiClient;
            BanFileMonitors = banFileMonitorsApiClient;
            ChatMessages = chatMessagesApiClient;
            Demos = demosApiClient;
            GameServers = gameServersApiClient;
            PlayerAnalytics = playerAnalyticsApiClient;
            Players = playersApiClient;
        }

        public IAdminActionsApiClient AdminActions { get; }
        public IBanFileMonitorsApiClient BanFileMonitors { get; }
        public IChatMessagesApiClient ChatMessages { get; }
        public IDemosApiClient Demos { get; }
        public IGameServersApiClient GameServers { get; }
        public IPlayerAnalyticsApiClient PlayerAnalytics { get; }
        public IPlayersApiClient Players { get; }
    }
}