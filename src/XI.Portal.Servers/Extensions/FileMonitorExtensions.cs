using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class FileMonitorExtensions
    {
        public static FileMonitorDto ToDto(this FileMonitors fileMonitor)
        {
            var fileMonitorDto = new FileMonitorDto
            {
                FileMonitorId = fileMonitor.FileMonitorId,
                FilePath = fileMonitor.FilePath,
                BytesRead = fileMonitor.BytesRead,
                LastRead = fileMonitor.LastRead,
                ServerId = fileMonitor.GameServerServer.ServerId,
                GameServer = fileMonitor.GameServerServer.ToDto()
            };

            return fileMonitorDto;
        }
    }
}