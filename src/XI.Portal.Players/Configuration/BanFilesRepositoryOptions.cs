using System;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Configuration
{
    internal class BanFilesRepositoryOptions : IBanFilesRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageContainerName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new ArgumentNullException(nameof(StorageConnectionString));

            if (string.IsNullOrWhiteSpace(StorageContainerName))
                throw new ArgumentNullException(nameof(StorageContainerName));
        }
    }
}