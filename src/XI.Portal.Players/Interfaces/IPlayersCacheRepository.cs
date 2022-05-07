using System.Threading.Tasks;
using XI.Portal.Players.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersCacheRepository
    {
        Task<PlayerCacheEntity> GetPlayer(GameType gameType, string guid);
        Task UpdatePlayer(PlayerCacheEntity model);
        Task RemoveOldEntries();
    }
}