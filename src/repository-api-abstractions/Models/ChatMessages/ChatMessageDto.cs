using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages
{
    public class ChatMessageDto
    {
        [JsonProperty]
        public Guid ChatLogId { get; set; }

        [JsonProperty]
        public Guid PlayerId { get; set; }

        [JsonProperty]
        public Guid ServerId { get; set; }

        [JsonProperty]
        public string ServerName { get; set; } = string.Empty;

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public DateTime Timestamp { get; set; }

        [JsonProperty]
        public string Username { get; set; } = string.Empty;

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatType ChatType { get; set; }

        [JsonProperty]
        public string Message { get; set; } = string.Empty;
    }
}
