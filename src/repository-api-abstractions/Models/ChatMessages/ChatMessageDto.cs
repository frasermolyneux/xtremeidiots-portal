using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid GameServerId { get; set; }
        public Guid PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}