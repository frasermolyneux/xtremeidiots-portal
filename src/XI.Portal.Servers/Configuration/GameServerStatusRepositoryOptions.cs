using System;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Configuration
{
    internal class GameServerStatusRepositoryOptions : IGameServerStatusRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageTableName { get; set; }

        public GeoLocationClientConfig GeoLocationClientConfiguration { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(StorageConnectionString);

            if (string.IsNullOrWhiteSpace(StorageTableName))
                throw new NullReferenceException(StorageTableName);

            if (GeoLocationClientConfiguration == null)
                throw new NullReferenceException(nameof(GeoLocationClientConfiguration));

            if (string.IsNullOrWhiteSpace(GeoLocationClientConfiguration.BaseUrl))
                throw new NullReferenceException(nameof(GeoLocationClientConfiguration.BaseUrl));

            if (string.IsNullOrWhiteSpace(GeoLocationClientConfiguration.ApiKey))
                throw new NullReferenceException(nameof(GeoLocationClientConfiguration.ApiKey));
        }
    }
}