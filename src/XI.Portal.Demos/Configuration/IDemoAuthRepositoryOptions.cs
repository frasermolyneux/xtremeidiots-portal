namespace XI.Portal.Demos.Configuration
{
    public interface IDemoAuthRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}