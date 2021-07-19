using System;
using XI.CommonTypes;

namespace XI.Portal.Bus.Models
{
    public class ChatMessage
    {
        public GameType GameType { get; set; }
        public Guid ServerId { get; set; }
        public ChatType ChatType { get; set; }
        public string Guid { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }
}