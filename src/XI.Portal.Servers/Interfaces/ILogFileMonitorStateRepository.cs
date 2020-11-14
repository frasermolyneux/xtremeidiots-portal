using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface ILogFileMonitorStateRepository
    {
        Task<List<LogFileMonitorStateDto>> GetLogFileMonitorStates(FileMonitorFilterModel filterModel);
        Task UpdateState(LogFileMonitorStateDto model);
        Task DeleteLogFileMonitorState(LogFileMonitorStateDto model);
    }
}