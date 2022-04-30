using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi
{
    public interface IPlayersApiClient
    {
        Task<PlayerDto?> GetPlayer(string accessToken, Guid id);
        Task<PlayerDto?> GetPlayerByGameType(string accessToken, string gameType, string guid);
        Task CreatePlayer(string accessToken, PlayerDto player);
        Task UpdatePlayer(string accessToken, PlayerDto player);
    }
}