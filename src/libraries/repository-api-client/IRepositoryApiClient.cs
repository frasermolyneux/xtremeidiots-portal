using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public interface IRepositoryApiClient
    {
        IAdminActionsApi AdminActions { get; }
        IBanFileMonitorsApi BanFileMonitors { get; }
        IChatMessagesApi ChatMessages { get; }
        IDataMaintenanceApi DataMaintenance { get; }
        IDemosAuthApi DemosAuth { get; }
        IDemosApi Demos { get; }
        IGameServersApi GameServers { get; }
        IGameServersEventsApi GameServersEvents { get; }
        IGameServersStatsApi GameServersStats { get; }
        ILivePlayersApi LivePlayers { get; }
        IMapsApi Maps { get; }
        IPlayerAnalyticsApi PlayerAnalytics { get; }
        IPlayersApi Players { get; }
        IRecentPlayersApi RecentPlayers { get; }
        IReportsApi Reports { get; }
        IUserProfileApi UserProfiles { get; }
    }
}