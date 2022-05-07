﻿using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi
{
    public interface IGameServersApiClient
    {
        Task<List<GameServerDto>?> GetGameServers(string accessToken, string[] gameTypes, Guid[] serverIds, string filterOption, int skipEntries, int takeEntries, string order);
        Task<GameServerDto?> GetGameServer(string accessToken, Guid serverId);
        Task CreateGameServer(string accessToken, GameServerDto gameServer);
        Task UpdateGameServer(string accessToken, GameServerDto gameServer);

        // Ban File Monitor Child Resources
        Task<BanFileMonitorDto> CreateBanFileMonitorForGameServer(string accessToken, Guid serverId, BanFileMonitorDto banFileMonitor);
        Task DeleteGameServer(string accessToken, Guid id);
    }
}