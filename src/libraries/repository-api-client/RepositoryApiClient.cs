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
using XtremeIdiots.Portal.RepositoryApiClient.UserProfileApi;

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public class RepositoryApiClient : IRepositoryApiClient
    {
        public RepositoryApiClient(
            IAdminActionsApiClient adminActionsApiClient,
            IBanFileMonitorsApiClient banFileMonitorsApiClient,
            IChatMessagesApiClient chatMessagesApiClient,
            IDataMaintenanceApiClient dataMaintenanceApiClient,
            IDemosAuthApiClient demosAuthApiClient,
            IDemosApiClient demosApiClient,
            IGameServersApiClient gameServersApiClient,
            IGameServersEventsApiClient gameServersEventsApiClient,
            IGameServersStatsApiClient gameServersStatsApiClient,
            ILivePlayersApiClient livePlayersApiClient,
            IMapsApiClient mapsApiClient,
            IPlayerAnalyticsApiClient playerAnalyticsApiClient,
            IPlayersApiClient playersApiClient,
            IUserProfileApiClient userProfileApiClient)
        {
            AdminActions = adminActionsApiClient;
            BanFileMonitors = banFileMonitorsApiClient;
            ChatMessages = chatMessagesApiClient;
            DataMaintenance = dataMaintenanceApiClient;
            DemosAuth = demosAuthApiClient;
            Demos = demosApiClient;
            GameServers = gameServersApiClient;
            GameServersEvents = gameServersEventsApiClient;
            GameServersStats = gameServersStatsApiClient;
            LivePlayers = livePlayersApiClient;
            Maps = mapsApiClient;
            PlayerAnalytics = playerAnalyticsApiClient;
            Players = playersApiClient;
            UserProfiles = userProfileApiClient;
        }

        public IAdminActionsApiClient AdminActions { get; }
        public IBanFileMonitorsApiClient BanFileMonitors { get; }
        public IChatMessagesApiClient ChatMessages { get; }
        public IDataMaintenanceApiClient DataMaintenance { get; }
        public IDemosAuthApiClient DemosAuth { get; }
        public IDemosApiClient Demos { get; }
        public IGameServersApiClient GameServers { get; }
        public IGameServersEventsApiClient GameServersEvents { get; }
        public IGameServersStatsApiClient GameServersStats { get; }
        public ILivePlayersApiClient LivePlayers { get; }
        public IMapsApiClient Maps { get; }
        public IPlayerAnalyticsApiClient PlayerAnalytics { get; }
        public IPlayersApiClient Players { get; }
        public IUserProfileApiClient UserProfiles { get; }
    }
}