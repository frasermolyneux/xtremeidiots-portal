using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Repository
{
    public interface IRconMonitorsRepository
    {
        Task<List<RconMonitors>> GetRconMonitors(ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<RconMonitors> GetRconMonitor(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task CreateRconMonitor(RconMonitors model);
        Task UpdateRconMonitor(Guid? id, RconMonitors model, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<bool> RconMonitorExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task RemoveRconMonitor(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
    }
}