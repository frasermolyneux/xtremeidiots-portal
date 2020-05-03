using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayersCacheRepository
    {
        Task<PlayerCacheEntity> GetPlayer(GameType gameType, string guid);
        Task UpdatePlayer(PlayerCacheEntity model);
        Task RemoveOldEntries();
    }
}