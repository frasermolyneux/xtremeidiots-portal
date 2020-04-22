using System;
using XI.Portal.Demos.Interfaces;

namespace XI.Portal.Demos.Configuration
{
    public class DemosRepositoryOptions : IDemosRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageContainerName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(StorageConnectionString);

            if (string.IsNullOrWhiteSpace(StorageContainerName))
                throw new NullReferenceException(StorageContainerName);
        }
    }
}