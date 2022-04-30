using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi
{
    public interface IBanFileMonitorsApiClient
    {
        Task<BanFileMonitorDto> GetBanFileMonitor(string accessToken, Guid banFileMonitorId);
        Task<List<BanFileMonitorDto>> GetBanFileMonitors(string accessToken, string[] gameTypes, Guid[] banFileMonitorIds, Guid? serverId, int skipEntries, int takeEntries, string order);
        Task<BanFileMonitorDto> UpdateBanFileMonitor(string accessToken, BanFileMonitorDto banFileMonitor);
        Task DeleteBanFileMonitor(string accessToken, Guid banFileMonitorId);
    }
}
