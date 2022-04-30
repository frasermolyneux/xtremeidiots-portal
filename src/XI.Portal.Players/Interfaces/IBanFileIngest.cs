using System.Threading.Tasks;

namespace XI.Portal.Players.Interfaces
{
    public interface IBanFileIngest
    {
        Task IngestBanFileDataForGame(string gameType, string remoteBanFileData);
    }
}