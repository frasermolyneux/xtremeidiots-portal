namespace XI.Portal.Demos.Configuration
{
    public interface IDemosRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageContainerName { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}