namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServerStatusRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}