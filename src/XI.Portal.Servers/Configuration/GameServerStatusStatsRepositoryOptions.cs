using System;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Configuration
{
    public class GameServerStatusStatsRepositoryOptions : IGameServerStatusStatsRepositoryOptions
    {
        public string StorageConnectionString { get; set; }
        public string StorageTableName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                throw new NullReferenceException(nameof(StorageConnectionString));

            if (string.IsNullOrWhiteSpace(StorageTableName))
                throw new NullReferenceException(nameof(StorageTableName));
        }
    }
}
