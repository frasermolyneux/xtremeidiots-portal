using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class BanFileMonitorExtensions
    {
        public static BanFileMonitorDto ToDto(this BanFileMonitors banFileMonitor)
        {
            var banFileMonitorDto = new BanFileMonitorDto
            {
                BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                FilePath = banFileMonitor.FilePath,
                RemoteFileSize = banFileMonitor.RemoteFileSize,
                LastSync = banFileMonitor.LastSync,
                ServerId = banFileMonitor.GameServerServer.ServerId,
                GameServer = banFileMonitor.GameServerServer.ToDto()
            };

            return banFileMonitorDto;
        }
    }
}