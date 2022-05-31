using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class GameServerStatDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Guid GameServerId { get; set; }

        [JsonProperty]
        public int PlayerCount { get; set; }

        [JsonProperty]
        public string MapName { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime Timestamp { get; set; }
    }
}
