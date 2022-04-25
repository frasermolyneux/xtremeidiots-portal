using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

public interface IPlayersApiClient
{
    Task<Player?> GetPlayer(string accessToken, Guid id);
    Task<Player?> GetPlayerByGameType(string accessToken, string gameType, string guid);
    Task CreatePlayer(string accessToken, Player player);
    Task UpdatePlayer(string accessToken, Player player);
}