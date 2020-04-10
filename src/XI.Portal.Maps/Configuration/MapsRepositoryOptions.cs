using System;

namespace XI.Portal.Maps.Configuration
{
    internal class MapsRepositoryOptions : IMapsRepositoryOptions
    {
        public string MapRedirectBaseUrl { get; set; }
        public string StorageConnectionString { get; set; }
        public string StorageTableName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(MapRedirectBaseUrl))
                throw new NullReferenceException(nameof(MapRedirectBaseUrl));

            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(nameof(StorageConnectionString));

            if (string.IsNullOrWhiteSpace(StorageTableName))
                throw new NullReferenceException(nameof(StorageTableName));
        }
    }
}