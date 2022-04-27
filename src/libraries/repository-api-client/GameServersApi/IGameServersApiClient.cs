using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;

public interface IGameServersApiClient
{
    Task<List<GameServerDto>?> GetGameServers(string accessToken);
    Task<GameServerDto?> GetGameServer(string accessToken, string id);
    Task CreateGameServer(string accessToken, GameServerDto gameServer);
    Task UpdateGameServer(string accessToken, GameServerDto gameServer);
}