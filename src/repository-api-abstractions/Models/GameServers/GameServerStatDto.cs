using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class GameServerStatDto
    {
        [JsonProperty]
        public Guid GameServerStatId { get; internal set; }

        [JsonProperty]
        public Guid GameServerId { get; internal set; }

        [JsonProperty]
        public int PlayerCount { get; internal set; }

        [JsonProperty]
        public string MapName { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime Timestamp { get; internal set; }
    }
}
