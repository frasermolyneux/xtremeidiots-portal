namespace XI.Portal.Demos.Interfaces
{
    public interface IDemoAuthRepositoryOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}