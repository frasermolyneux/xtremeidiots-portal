using System;

namespace XI.AzureTableLogging.Configuration
{
    internal class AzureTableLoggerOptions : IAzureTableLoggerOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageContainerName { get; set; }
        public bool CreateTableIfNotExists { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(nameof(StorageConnectionString));

            if (string.IsNullOrWhiteSpace(StorageContainerName))
                throw new NullReferenceException(nameof(StorageContainerName));
        }
    }
}