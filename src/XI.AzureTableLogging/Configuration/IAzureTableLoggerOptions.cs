namespace XI.AzureTableLogging.Configuration
{
    public interface IAzureTableLoggerOptions
    {
        string ConnectionString { get; set; }
        string LogTableName { get; set; }
        bool CreateTableIfNotExists { get; set; }
        void Validate();
    }
}