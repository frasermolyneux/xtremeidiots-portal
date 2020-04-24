using System;
using XI.CommonTypes;
using XI.Portal.Data.Legacy.CommonTypes;

namespace XI.Portal.Players.Dto
{
    public class AdminActionDto
    {
        public Guid AdminActionId { get; set; }
        public Guid PlayerId { get; set; }
        public GameType GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        public AdminActionType Type { get; set; }
        public string Text { get; set; }
        public DateTime? Expires { get; set; }
        public int ForumTopicId { get; set; }
        public DateTime Created { get; set; }

        public string AdminId { get; set; }
        public string AdminName { get; set; }
    }
}