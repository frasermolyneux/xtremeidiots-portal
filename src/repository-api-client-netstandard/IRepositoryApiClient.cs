using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.AdminActionsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DataMaintenanceApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosAuthApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.DemosRepositoryApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.MapsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayerAnalyticsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
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
        IMapsApiClient Maps { get; }
        IPlayerAnalyticsApiClient PlayerAnalytics { get; }
        IPlayersApiClient Players { get; }
    }
}