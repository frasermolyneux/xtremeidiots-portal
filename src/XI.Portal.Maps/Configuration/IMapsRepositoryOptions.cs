namespace XI.Portal.Maps.Configuration
{
    public interface IMapsRepositoryOptions
    {
        string MapRedirectBaseUrl { get; set; }
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }

        void Validate();
    }
}