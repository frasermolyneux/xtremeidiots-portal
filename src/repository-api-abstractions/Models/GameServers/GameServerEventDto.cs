using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class GameServerEventDto
    {
        [JsonProperty]
        public Guid GameServerEventId { get; internal set; }

        [JsonProperty]
        public Guid GameServerId { get; internal set; }

        [JsonProperty]
        public DateTime Timestamp { get; internal set; }

        [JsonProperty]
        public string EventType { get; internal set; } = string.Empty;

        [JsonProperty]
        public string EventData { get; internal set; } = string.Empty;
    }
}