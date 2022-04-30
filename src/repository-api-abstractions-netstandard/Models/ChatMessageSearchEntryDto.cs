using System;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models
{
    public class ChatMessageSearchEntryDto
    {
        public Guid ChatLogId { get; set; }
        public Guid PlayerId { get; set; }
        public Guid ServerId { get; set; }
        public string ServerName { get; set; }
        public string GameType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string ChatType { get; set; }
        public string Message { get; set; }
    }
}
