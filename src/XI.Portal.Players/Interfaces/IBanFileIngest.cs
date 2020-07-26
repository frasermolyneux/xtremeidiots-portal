using System.Threading.Tasks;
using XI.CommonTypes;

namespace XI.Portal.Players.Interfaces
{
    public interface IBanFileIngest
    {
        Task IngestBanFileDataForGame(GameType gameType, string remoteBanFileData);
    }
}