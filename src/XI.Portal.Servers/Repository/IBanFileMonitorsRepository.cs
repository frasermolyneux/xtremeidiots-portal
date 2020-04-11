using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Repository
{
    public interface IBanFileMonitorsRepository
    {
        Task<List<BanFileMonitors>> GetBanFileMonitors(ClaimsPrincipal user);
        Task<BanFileMonitors> GetBanFileMonitor(Guid? id, ClaimsPrincipal user);
        Task CreateBanFileMonitor(BanFileMonitors model);
        Task UpdateBanFileMonitor(Guid? id, BanFileMonitors model, ClaimsPrincipal user);
        Task<bool> BanFileMonitorExists(Guid id, ClaimsPrincipal user);
        Task RemoveBanFileMonitor(Guid id, ClaimsPrincipal user);
    }
}