using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;

public interface IGameServersEventsApiClient
{
    Task CreateGameServerEvent(string accessToken, string id, GameServerEvent gameServerEvent);
}