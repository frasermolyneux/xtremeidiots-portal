namespace XI.Portal.Players.Interfaces
{
    public interface IBanFilesRepository
    {
        Task RegenerateBanFileForGame(string gameType);
        Task<long> GetBanFileSizeForGame(string gameType);
        Task<Stream> GetBanFileForGame(string gameType);
    }
}