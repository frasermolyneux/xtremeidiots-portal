using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Interfaces
{
    public interface IGameServerStatusStatsRepository
    {
        Task<List<GameServerStatusStatsDto>> GetGameServerStatusStats(GameServerStatusStatsFilterModel filterModel);
        Task UpdateEntry(GameServerStatusStatsDto model);
        Task DeleteGameServerStatusStats(Guid serverId);
        Task RemoveOldEntries(List<Guid> serverIds);
    }
}