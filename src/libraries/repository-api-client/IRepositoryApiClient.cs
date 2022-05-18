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

namespace XtremeIdiots.Portal.RepositoryApiClient
{
    public interface IRepositoryApiClient
    {
        IAdminActionsApiClient AdminActions { get; }
        IBanFileMonitorsApiClient BanFileMonitors { get; }
        IChatMessagesApiClient ChatMessages { get; }
        IDataMaintenanceApiClient DataMaintenance { get; }
        IDemosAuthApiClient DemosAuth { get; }
        IDemosApiClient Demos { get; }
        IGameServersApiClient GameServers { get; }
        IGameServersEventsApiClient GameServersEvents { get; }
        IGameServersStatsApiClient GameServersStats { get; }
        ILivePlayersApiClient LivePlayers { get; }
        IMapsApiClient Maps { get; }
        IPlayerAnalyticsApiClient PlayerAnalytics { get; }
        IPlayersApiClient Players { get; }
    }
}