using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Repository
{
    public interface IFileMonitorsRepository
    {
        Task<List<FileMonitors>> GetFileMonitors(ClaimsPrincipal user);
        Task<FileMonitors> GetFileMonitor(Guid? id, ClaimsPrincipal user);
        Task CreateFileMonitor(FileMonitors model);
        Task UpdateFileMonitor(Guid? id, FileMonitors model, ClaimsPrincipal user);
        Task<bool> FileMonitorExists(Guid id, ClaimsPrincipal user);
        Task RemoveFileMonitor(Guid id, ClaimsPrincipal user);
    }
}