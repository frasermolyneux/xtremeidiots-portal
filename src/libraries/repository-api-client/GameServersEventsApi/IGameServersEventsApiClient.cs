using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;

public interface IGameServersEventsApiClient
{
    Task CreateGameServerEvent(GameServerEventDto gameServerEvent);
}