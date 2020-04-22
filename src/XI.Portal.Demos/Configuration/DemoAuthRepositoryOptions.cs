using System;
using XI.Portal.Demos.Interfaces;

namespace XI.Portal.Demos.Configuration
{
    internal class DemoAuthRepositoryOptions : IDemoAuthRepositoryOptions
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