using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

public interface IPlayersApiClient
{
    Task<PlayerApiDto?> GetPlayer(string accessToken, Guid id);
    Task<PlayerApiDto?> GetPlayerByGameType(string accessToken, string gameType, string guid);
    Task CreatePlayer(string accessToken, PlayerApiDto player);
    Task UpdatePlayer(string accessToken, PlayerApiDto player);
}