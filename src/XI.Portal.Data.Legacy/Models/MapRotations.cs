using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class MapRotations
    {
        public Guid MapRotationId { get; set; }
        public string GameMode { get; set; }
        public Guid GameServerServerId { get; set; }
        public Guid MapMapId { get; set; }

        public virtual GameServers GameServerServer { get; set; }
        public virtual Maps MapMap { get; set; }
    }
}