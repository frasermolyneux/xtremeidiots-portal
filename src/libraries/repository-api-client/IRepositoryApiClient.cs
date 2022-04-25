using XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersSecretsApi;
using XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient;

public interface IRepositoryApiClient
{
    IChatMessagesApiClient ChatMessages { get; }
    IDataMaintenanceApiClient DataMaintenance { get; }
    IGameServersApiClient GameServers { get; }
    IGameServersEventsApiClient GameServersEvents { get; }
    IGameServersSecretsApiClient GameServersSecrets { get; }
    IPlayersApiClient Players { get; }
}