using System;

namespace XI.Portal.Maps.Configuration
{
    internal class MapImageRepositoryOptions : IMapImageRepositoryOptions
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