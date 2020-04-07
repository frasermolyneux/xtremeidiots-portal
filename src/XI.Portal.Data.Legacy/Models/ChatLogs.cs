using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class ChatLogs
    {
        public Guid ChatLogId { get; set; }
        public string Username { get; set; }
        public int ChatType { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? GameServerServerId { get; set; }
        public Guid? PlayerPlayerId { get; set; }

        public virtual GameServers GameServerServer { get; set; }
        public virtual Player2 PlayerPlayer { get; set; }
    }
}