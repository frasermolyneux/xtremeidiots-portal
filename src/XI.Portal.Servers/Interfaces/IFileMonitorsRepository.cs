using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IFileMonitorsRepository
    {
        Task<int> GetFileMonitorsCount(FileMonitorFilterModel filterModel);
        Task<List<FileMonitorDto>> GetFileMonitors(FileMonitorFilterModel filterModel);
        Task<FileMonitorDto> GetFileMonitor(Guid fileMonitorId);
        Task CreateFileMonitor(FileMonitorDto fileMonitorDto);
        Task UpdateFileMonitor(FileMonitorDto fileMonitorDto);
        Task DeleteFileMonitor(Guid fileMonitorId);
    }
}