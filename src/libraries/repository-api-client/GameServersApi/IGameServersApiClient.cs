using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;

public interface IGameServersApiClient
{
    Task<List<GameServerApiDto>?> GetGameServers(string accessToken);
    Task<GameServerApiDto?> GetGameServer(string accessToken, string id);
    Task CreateGameServer(string accessToken, GameServerApiDto gameServer);
    Task UpdateGameServer(string accessToken, GameServerApiDto gameServer);
}