using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class BanFileMonitorExtensions
    {
        public static BanFileMonitorDto ToDto(this BanFileMonitor banFileMonitor)
        {
            var dto = new BanFileMonitorDto
            {
                BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                FilePath = banFileMonitor.FilePath,
                RemoteFileSize = banFileMonitor.RemoteFileSize,
                LastSync = banFileMonitor.LastSync,

            };

            if (banFileMonitor.GameServerServerId != null)
            {
                dto.ServerId = (Guid)banFileMonitor.GameServerServerId;
                dto.GameType = banFileMonitor.GameServerServer.GameType.ToGameType();
            }

            return dto;
        }
    }
}
