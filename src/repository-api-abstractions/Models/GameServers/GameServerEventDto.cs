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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string EventType { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonProperty]
        public string? EventData { get; internal set; }

        [JsonProperty]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GameServerDto GameServer { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}