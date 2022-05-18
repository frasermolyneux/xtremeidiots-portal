﻿using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersStatsApi
{
    public interface IGameServersStatsApiClient
    {
        Task<List<GameServerStatDto>?> CreateGameServerStats(List<GameServerStatDto> gameServerStatDtos);
    }
}