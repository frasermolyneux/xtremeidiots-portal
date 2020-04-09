namespace XI.AzureTableLogging.Configuration
{
    public interface IAzureTableLoggerOptions
    {
        string StorageConnectionString { get; set; }
        string StorageContainerName { get; set; }
        bool CreateTableIfNotExists { get; set; }

        void Validate();
    }
}