using System;

namespace XI.Portal.AzureTableLogging
{
    public class AzureTableLoggerOptions
    {
        public string ConnectionString { get; set; }
        public string LogTableName { get; set; }
        public bool CreateTableIfNotExists { get; set; } = false;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new NullReferenceException(nameof(ConnectionString));

            if (string.IsNullOrWhiteSpace(LogTableName))
                throw new NullReferenceException(nameof(LogTableName));
        }
    }
}