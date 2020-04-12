using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public interface IBanFileMonitorsRepository
    {
        Task<List<BanFileMonitors>> GetBanFileMonitors(ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<BanFileMonitors> GetBanFileMonitor(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task CreateBanFileMonitor(BanFileMonitors model);
        Task UpdateBanFileMonitor(Guid? id, BanFileMonitors model, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<bool> BanFileMonitorExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task RemoveBanFileMonitor(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<List<BanFileMonitorStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims);
    }
}