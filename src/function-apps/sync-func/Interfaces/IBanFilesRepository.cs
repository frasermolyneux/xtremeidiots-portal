using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.SyncFunc.Interfaces
{
    public interface IBanFilesRepository
    {
        Task RegenerateBanFileForGame(GameType gameType);
        Task<long> GetBanFileSizeForGame(GameType gameType);
        Task<Stream> GetBanFileForGame(GameType gameType);
    }
}