using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class RconMonitorExtensions
    {
        public static RconMonitorDto ToDto(this RconMonitors rconMonitor)
        {
            var rconMonitorDto = new RconMonitorDto
            {
                RconMonitorId = rconMonitor.RconMonitorId,
                LastUpdated = rconMonitor.LastUpdated,
                MonitorMapRotation = rconMonitor.MonitorMapRotation,
                MapRotationLastUpdated = rconMonitor.MapRotationLastUpdated,
                MonitorPlayers = rconMonitor.MonitorPlayers,
                MonitorPlayerIps = rconMonitor.MonitorPlayerIps,
                ServerId = rconMonitor.GameServerServer.ServerId,
                GameServer = rconMonitor.GameServerServer.ToDto()
            };

            return rconMonitorDto;
        }
    }
}