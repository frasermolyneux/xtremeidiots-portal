using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Configuration
{
    public class PlayerLocationsRepositoryOptions : IPlayerLocationsRepositoryOptions
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