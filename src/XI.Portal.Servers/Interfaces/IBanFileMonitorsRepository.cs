using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IBanFileMonitorsRepository
    {
        Task<int> GetBanFileMonitorsCount(BanFileMonitorFilterModel filterModel);
        Task<List<BanFileMonitorDto>> GetBanFileMonitors(BanFileMonitorFilterModel filterModel);
        Task<BanFileMonitorDto> GetBanFileMonitor(Guid banFileMonitorId);
        Task CreateBanFileMonitor(BanFileMonitorDto banFileMonitorDto);
        Task UpdateBanFileMonitor(BanFileMonitorDto banFileMonitorDto);
        Task DeleteBanFileMonitor(Guid banFileMonitorId);
    }
}