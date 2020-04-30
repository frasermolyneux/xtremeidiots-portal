using System;
using System.ComponentModel;

namespace XI.Portal.Servers.Dto
{
    public class RconMonitorDto
    {
        public Guid RconMonitorId { get; set; }
        [DisplayName("Last Updated")] public DateTime LastUpdated { get; set; }
        [DisplayName("Monitor Map Rotation")] public bool MonitorMapRotation { get; set; }

        [DisplayName("Map Rotation Last Updated")]
        public DateTime MapRotationLastUpdated { get; set; }

        [DisplayName("Monitor Players")] public bool MonitorPlayers { get; set; }
        [DisplayName("Monitor Player IPs")] public bool MonitorPlayerIps { get; set; }
        [DisplayName("Server")] public Guid ServerId { get; set; }
        [DisplayName("Server")] public GameServerDto GameServer { get; set; }
    }
}