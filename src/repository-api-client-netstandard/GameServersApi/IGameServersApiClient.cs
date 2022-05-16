using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi
{
    public interface IGameServersApiClient
    {
        Task<List<GameServerDto>?> GetGameServers(string[] gameTypes, Guid[] serverIds, string filterOption, int skipEntries, int takeEntries, string order);
        Task<GameServerDto?> GetGameServer(Guid serverId);
        Task CreateGameServer(GameServerDto gameServer);
        Task UpdateGameServer(GameServerDto gameServer);

        // Ban File Monitor Child Resources
        Task<BanFileMonitorDto> CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor);
        Task DeleteGameServer(Guid id);
    }
}