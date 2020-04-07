using System;
using System.ComponentModel;

namespace XI.Portal.Data.Legacy.Models
{
    public class RconMonitors
    {
        public Guid RconMonitorId { get; set; }

        [DisplayName("Last Updated")] public DateTime LastUpdated { get; set; }

        [DisplayName("Monitor Map Rotation")] public bool MonitorMapRotation { get; set; }

        [DisplayName("Map Rotation Last Updated")]
        public DateTime MapRotationLastUpdated { get; set; }

        [DisplayName("Monitor Players")] public bool MonitorPlayers { get; set; }

        [DisplayName("Monitor Player IPs")] public bool MonitorPlayerIps { get; set; }

        [Obsolete] private string LastError { get; set; }
        [DisplayName("Server")] public Guid? GameServerServerId { get; set; }

        [DisplayName("Server")] public virtual GameServers GameServerServer { get; set; }
    }
}