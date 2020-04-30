using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class RconMonitors
    {
        public Guid RconMonitorId { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool MonitorMapRotation { get; set; }
        public DateTime MapRotationLastUpdated { get; set; }
        public bool MonitorPlayers { get; set; }
        public bool MonitorPlayerIps { get; set; }
        [Obsolete] private string LastError { get; set; }
        public Guid GameServerServerId { get; set; }
        public virtual GameServers GameServerServer { get; set; }
    }
}