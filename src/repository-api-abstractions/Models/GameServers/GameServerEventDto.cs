using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class GameServerEventDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Guid GameServerId { get; set; }

        [JsonProperty]
        public DateTime Timestamp { get; set; }

        [JsonProperty]
        public string EventType { get; set; } = string.Empty;

        [JsonProperty]
        public string EventData { get; set; } = string.Empty;
    }
}