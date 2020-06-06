namespace XI.Portal.Servers.Interfaces
{
    public interface ILogFileMonitorStateRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}