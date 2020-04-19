using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Servers.Dto;

namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServerStatusRepository
    {
        Task<GameServerStatusDto> GetStatus(Guid serverId, ClaimsPrincipal user, IEnumerable<string> requiredClaims, TimeSpan cacheCutoff);
        Task UpdateStatus(Guid id, GameServerStatusDto model);
    }
}