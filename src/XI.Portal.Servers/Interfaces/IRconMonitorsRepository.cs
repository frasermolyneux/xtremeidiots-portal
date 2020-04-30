using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IRconMonitorsRepository
    {
        Task<int> GetRconMonitorsCount(RconMonitorFilterModel filterModel);
        Task<List<RconMonitorDto>> GetRconMonitors(RconMonitorFilterModel filterModel);
        Task<RconMonitorDto> GetRconMonitor(Guid rconMonitorId);
        Task CreateRconMonitor(RconMonitorDto rconMonitorDto);
        Task UpdateRconMonitor(RconMonitorDto rconMonitorDto);
        Task DeleteRconMonitor(Guid rconMonitorId);
    }
}