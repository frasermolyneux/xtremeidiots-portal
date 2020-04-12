using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Repository
{
    public interface IFileMonitorsRepository
    {
        Task<List<FileMonitors>> GetFileMonitors(ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<FileMonitors> GetFileMonitor(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task CreateFileMonitor(FileMonitors model);
        Task UpdateFileMonitor(Guid? id, FileMonitors model, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<bool> FileMonitorExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task RemoveFileMonitor(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
    }
}