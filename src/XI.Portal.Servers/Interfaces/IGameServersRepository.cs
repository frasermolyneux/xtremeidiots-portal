using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServersRepository
    {
        Task<int> GetGameServersCount(GameServerFilterModel filterModel);
        Task<List<GameServerDto>> GetGameServers(GameServerFilterModel filterModel);
        Task<GameServerDto> GetGameServer(Guid gameServerId);
        Task CreateGameServer(GameServerDto gameServerDto);
        Task UpdateGameServer(GameServerDto gameServerDto);
        Task DeleteGameServer(Guid gameServerId);
    }
}