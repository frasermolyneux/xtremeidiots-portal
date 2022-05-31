using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;

public interface IGameServersEventsApiClient
{
    Task CreateGameServerEvent(GameServerEventDto gameServerEvent);
}