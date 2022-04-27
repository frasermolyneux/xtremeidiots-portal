using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

public interface IPlayersApiClient
{
    Task<PlayerDto?> GetPlayer(string accessToken, Guid id);
    Task<PlayerDto?> GetPlayerByGameType(string accessToken, string gameType, string guid);
    Task CreatePlayer(string accessToken, PlayerDto player);
    Task UpdatePlayer(string accessToken, PlayerDto player);
}