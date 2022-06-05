using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class CreateGameServerEventDto
    {
        public CreateGameServerEventDto(Guid gameServerId, string eventType, string eventData)
        {
            GameServerId = gameServerId;
            EventType = eventType;
            EventData = eventData;
        }

        [JsonProperty]
        public Guid GameServerId { get; set; }

        [JsonProperty]
        public string EventType { get; set; }

        [JsonProperty]
        public string EventData { get; set; }
    }
}
