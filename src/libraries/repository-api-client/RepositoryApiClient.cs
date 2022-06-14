using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public class RepositoryApiClient : IRepositoryApiClient
    {
        public RepositoryApiClient(
            IAdminActionsApi adminActionsApiClient,
            IBanFileMonitorsApi banFileMonitorsApiClient,
            IChatMessagesApi chatMessagesApiClient,
            IDataMaintenanceApi dataMaintenanceApiClient,
            IDemosApi demosApiClient,
            IGameServersApi gameServersApiClient,
            IGameServersEventsApi gameServersEventsApiClient,
            IGameServersStatsApi gameServersStatsApiClient,
            ILivePlayersApi livePlayersApiClient,
            IMapsApi mapsApiClient,
            IPlayerAnalyticsApi playerAnalyticsApiClient,
            IPlayersApi playersApiClient,
            IRecentPlayersApi recentPlayersApiClient,
            IReportsApi reportsApiClient,
            IUserProfileApi userProfileApiClient)
        {
            AdminActions = adminActionsApiClient;
            BanFileMonitors = banFileMonitorsApiClient;
            ChatMessages = chatMessagesApiClient;
            DataMaintenance = dataMaintenanceApiClient;
            Demos = demosApiClient;
            GameServers = gameServersApiClient;
            GameServersEvents = gameServersEventsApiClient;
            GameServersStats = gameServersStatsApiClient;
            LivePlayers = livePlayersApiClient;
            Maps = mapsApiClient;
            PlayerAnalytics = playerAnalyticsApiClient;
            Players = playersApiClient;
            RecentPlayers = recentPlayersApiClient;
            Reports = reportsApiClient;
            UserProfiles = userProfileApiClient;
        }

        public IAdminActionsApi AdminActions { get; }
        public IBanFileMonitorsApi BanFileMonitors { get; }
        public IChatMessagesApi ChatMessages { get; }
        public IDataMaintenanceApi DataMaintenance { get; }
        public IDemosApi Demos { get; }
        public IGameServersApi GameServers { get; }
        public IGameServersEventsApi GameServersEvents { get; }
        public IGameServersStatsApi GameServersStats { get; }
        public ILivePlayersApi LivePlayers { get; }
        public IMapsApi Maps { get; }
        public IPlayerAnalyticsApi PlayerAnalytics { get; }
        public IPlayersApi Players { get; }
        public IRecentPlayersApi RecentPlayers { get; }
        public IReportsApi Reports { get; }
        public IUserProfileApi UserProfiles { get; }
    }
}