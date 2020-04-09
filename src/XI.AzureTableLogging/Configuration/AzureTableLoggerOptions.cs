using System;

namespace XI.AzureTableLogging.Configuration
{
    internal class AzureTableLoggerOptions : IAzureTableLoggerOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageTableName { get; set; }
        public bool CreateTableIfNotExists { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(nameof(StorageConnectionString));

            if (string.IsNullOrWhiteSpace(StorageTableName))
                throw new NullReferenceException(nameof(StorageTableName));
        }
    }
}