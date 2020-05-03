namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServerStatusStatsRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}