using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Repository
{
    public interface IGameServersRepository
    {
        Task<List<GameServers>> GetGameServers(ClaimsPrincipal user);
        Task<GameServers> GetGameServer(Guid? id, ClaimsPrincipal user);
        Task CreateGameServer(GameServers model);
        Task UpdateGameServer(Guid? id, GameServers model, ClaimsPrincipal user);
        Task<bool> GameServerExists(Guid id, ClaimsPrincipal user);
        Task RemoveGameServer(Guid id, ClaimsPrincipal user);
        Task<List<GameServers>> GetGameServersForCredentials(ClaimsPrincipal user);
    }
}