using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.AdminActionsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DataMaintenanceApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.MapsApi;
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
            IDataMaintenanceApiClient dataMaintenanceApiClient,
            IDemosApiClient demosApiClient,
            IGameServersApiClient gameServersApiClient,
            IMapsApiClient mapsApiClient,
            IPlayerAnalyticsApiClient playerAnalyticsApiClient,
            IPlayersApiClient playersApiClient)
        {
            AdminActions = adminActionsApiClient;
            BanFileMonitors = banFileMonitorsApiClient;
            ChatMessages = chatMessagesApiClient;
            DataMaintenance = dataMaintenanceApiClient;
            Demos = demosApiClient;
            GameServers = gameServersApiClient;
            Maps = mapsApiClient;
            PlayerAnalytics = playerAnalyticsApiClient;
            Players = playersApiClient;
        }

        public IAdminActionsApiClient AdminActions { get; }
        public IBanFileMonitorsApiClient BanFileMonitors { get; }
        public IChatMessagesApiClient ChatMessages { get; }
        public IDataMaintenanceApiClient DataMaintenance { get; }
        public IDemosApiClient Demos { get; }
        public IGameServersApiClient GameServers { get; }
        public IMapsApiClient Maps { get; }
        public IPlayerAnalyticsApiClient PlayerAnalytics { get; }
        public IPlayersApiClient Players { get; }
    }
}