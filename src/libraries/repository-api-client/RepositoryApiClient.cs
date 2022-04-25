using XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersSecretsApi;
using XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient;

public class RepositoryApiClient : IRepositoryApiClient
{
    public RepositoryApiClient(
        IChatMessagesApiClient chatMessagesApiClient,
        IDataMaintenanceApiClient dataMaintenanceApiClient,
        IGameServersApiClient gameServersApiClient,
        IGameServersEventsApiClient gameServersEventsApiClient,
        IGameServersSecretsApiClient gameServersSecretsApiClient,
        IPlayersApiClient playersApiClient)
    {
        ChatMessages = chatMessagesApiClient;
        DataMaintenance = dataMaintenanceApiClient;
        GameServers = gameServersApiClient;
        GameServersEvents = gameServersEventsApiClient;
        GameServersSecrets = gameServersSecretsApiClient;
        Players = playersApiClient;
    }

    public IChatMessagesApiClient ChatMessages { get; }
    public IDataMaintenanceApiClient DataMaintenance { get; }
    public IGameServersApiClient GameServers { get; }
    public IGameServersEventsApiClient GameServersEvents { get; }
    public IGameServersSecretsApiClient GameServersSecrets { get; }
    public IPlayersApiClient Players { get; }
}