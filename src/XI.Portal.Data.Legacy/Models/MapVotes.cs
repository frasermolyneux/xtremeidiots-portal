using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class MapVotes
    {
        public Guid MapVoteId { get; set; }
        public bool Like { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid MapMapId { get; set; }
        public Guid PlayerPlayerId { get; set; }

        public virtual Maps MapMap { get; set; }
        public virtual Player2 PlayerPlayer { get; set; }
    }
}