using System.IO;
using System.Threading.Tasks;
using XI.CommonTypes;

namespace XI.Portal.Players.Interfaces
{
    public interface IBanFilesRepository
    {
        Task RegenerateBanFileForGame(GameType gameType);
        Task<long> GetBanFileSizeForGame(GameType gameType);
        Task<Stream> GetBanFileForGame(GameType gameType);
    }
}