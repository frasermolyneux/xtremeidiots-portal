using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Interfaces
{
    public interface ILogFileMonitorStateRepository
    {
        Task<List<LogFileMonitorStateDto>> GetLogFileMonitorStates();
        Task UpdateState(LogFileMonitorStateDto model);
    }
}