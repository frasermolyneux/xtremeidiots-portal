using System;
using XI.Portal.Data.Legacy.CommonTypes;

namespace XI.Portal.Data.Legacy.Models
{
    public class AdminActions
    {
        public Guid AdminActionId { get; set; }
        public AdminActionType Type { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Expires { get; set; }
        public int ForumTopicId { get; set; }
        public string AdminId { get; set; }
        public Guid? PlayerPlayerId { get; set; }

        public virtual AspNetUsers Admin { get; set; }
        public virtual Player2 PlayerPlayer { get; set; }
    }
}