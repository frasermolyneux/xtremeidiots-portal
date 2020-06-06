using System;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Configuration
{
    internal class LogFileMonitorStateRepositoryOptions : ILogFileMonitorStateRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageTableName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(StorageConnectionString);

            if (string.IsNullOrWhiteSpace(StorageTableName))
                throw new NullReferenceException(StorageTableName);
        }
    }
}