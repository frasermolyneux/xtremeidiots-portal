using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages
{
    public class CreateChatMessageDto
    {
        public CreateChatMessageDto(Guid gameServerId, Guid playerId, ChatType chatType, string username, string message, DateTime timestamp)
        {
            GameServerId = gameServerId;
            PlayerId = playerId;
            ChatType = chatType;
            Username = username;
            Message = message;
            Timestamp = timestamp;
        }

        [JsonProperty]
        public Guid GameServerId { get; private set; }

        [JsonProperty]
        public Guid PlayerId { get; private set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatType ChatType { get; private set; }

        [JsonProperty]
        public string Username { get; private set; }

        [JsonProperty]
        public string Message { get; private set; }

        [JsonProperty]
        public DateTime Timestamp { get; private set; }
    }
}
