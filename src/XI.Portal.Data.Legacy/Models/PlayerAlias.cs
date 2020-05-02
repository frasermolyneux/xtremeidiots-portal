using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class PlayerAlias
    {
        public Guid PlayerAliasId { get; set; }
        public string Name { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUsed { get; set; }
        public Guid PlayerPlayerId { get; set; }

        public virtual Player2 PlayerPlayer { get; set; }
    }
}