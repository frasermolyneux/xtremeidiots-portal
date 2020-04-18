namespace XI.Portal.Servers.Configuration
{
    public interface IGameServerStatusRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}