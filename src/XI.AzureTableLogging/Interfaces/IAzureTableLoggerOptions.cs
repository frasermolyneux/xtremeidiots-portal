namespace XI.AzureTableLogging.Interfaces
{
    public interface IAzureTableLoggerOptions
    {
        string StorageConnectionString { get; set; }
        string StorageTableName { get; set; }
        bool CreateTableIfNotExists { get; set; }

        void Validate();
    }
}