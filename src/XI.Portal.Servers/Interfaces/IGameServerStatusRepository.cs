using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServerStatusRepository
    {
        Task<PortalGameServerStatusDto> GetStatus(Guid serverId, ClaimsPrincipal user, string[] requiredClaims, TimeSpan cacheCutoff);
        Task UpdateStatus(Guid id, PortalGameServerStatusDto model);
        Task<List<PortalGameServerStatusDto>> GetAllStatusModels(ClaimsPrincipal user, string[] requiredClaims, TimeSpan cacheCutoff);
    }
}