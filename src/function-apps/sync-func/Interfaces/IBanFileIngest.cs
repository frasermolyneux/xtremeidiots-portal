namespace XtremeIdiots.Portal.SyncFunc.Interfaces
{
    public interface IBanFileIngest
    {
        Task IngestBanFileDataForGame(string gameType, string remoteBanFileData);
    }
}