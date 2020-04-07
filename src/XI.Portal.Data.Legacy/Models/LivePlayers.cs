using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class LivePlayers
    {
        public Guid LivePlayerId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Ping { get; set; }
        public string Team { get; set; }
        public TimeSpan Time { get; set; }
        public Guid? GameServerServerId { get; set; }

        public virtual GameServers GameServerServer { get; set; }
    }
}