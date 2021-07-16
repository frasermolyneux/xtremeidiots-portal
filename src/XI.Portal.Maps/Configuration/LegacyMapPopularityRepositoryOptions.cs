using System;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Maps.Configuration
{
    public class LegacyMapPopularityRepositoryOptions : ILegacyMapPopularityRepositoryOptions
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