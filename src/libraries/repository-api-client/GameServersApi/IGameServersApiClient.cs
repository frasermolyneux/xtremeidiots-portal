using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;

public interface IGameServersApiClient
{
    Task<List<GameServer>?> GetGameServers(string accessToken);
    Task<GameServer?> GetGameServer(string accessToken, string id);
    Task CreateGameServer(string accessToken, GameServer gameServer);
    Task UpdateGameServer(string accessToken, GameServer gameServer);
}