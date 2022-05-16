using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi
{
    public interface IBanFileMonitorsApiClient
    {
        Task<BanFileMonitorDto> GetBanFileMonitor(Guid banFileMonitorId);
        Task<List<BanFileMonitorDto>> GetBanFileMonitors(string[] gameTypes, Guid[] banFileMonitorIds, Guid? serverId, int skipEntries, int takeEntries, string order);
        Task<BanFileMonitorDto> UpdateBanFileMonitor(BanFileMonitorDto banFileMonitor);
        Task DeleteBanFileMonitor(Guid banFileMonitorId);
    }
}
