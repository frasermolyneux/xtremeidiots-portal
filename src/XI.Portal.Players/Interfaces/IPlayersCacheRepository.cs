using XI.Portal.Players.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersCacheRepository
    {
        Task<PlayerCacheEntity> GetPlayer(GameType gameType, string guid);
        Task UpdatePlayer(PlayerCacheEntity model);
        Task RemoveOldEntries();
    }
}