using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServerStatusRepository
    {
        Task<PortalGameServerStatusDto> GetStatus(Guid serverId, TimeSpan cacheCutoff);
        Task UpdateStatus(Guid id, PortalGameServerStatusDto model);
        Task<List<PortalGameServerStatusDto>> GetAllStatusModels(GameServerStatusFilterModel filterModel, TimeSpan cacheCutoff);
        Task DeleteStatusModel(PortalGameServerStatusDto model);
    }
}