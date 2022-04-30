using System;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string GameServerId { get; set; }
        public Guid PlayerId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}