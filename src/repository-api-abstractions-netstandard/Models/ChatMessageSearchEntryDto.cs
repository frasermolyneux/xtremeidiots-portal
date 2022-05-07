using System;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class ChatMessageSearchEntryDto
    {
        public Guid ChatLogId { get; set; }
        public Guid PlayerId { get; set; }
        public Guid ServerId { get; set; }
        public string ServerName { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatType ChatType { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
