using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServersRepository
    {
        Task<List<GameServerDto>> GetGameServers(ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<GameServerDto> GetGameServer(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task CreateGameServer(GameServerDto model);
        Task UpdateGameServer(Guid? id, GameServerDto model, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<bool> GameServerExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);

        Task RemoveGameServer(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims);
        Task<List<GameServerStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims);
        Task<List<string>> GetGameServerBanners();
    }
}