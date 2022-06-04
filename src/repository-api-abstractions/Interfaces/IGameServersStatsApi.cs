using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IGameServersStatsApi
    {
        Task<List<GameServerStatDto>?> CreateGameServerStats(List<GameServerStatDto> gameServerStatDtos);
        Task<List<GameServerStatDto>> GetGameServerStatusStats(Guid serverId, DateTime cutoff);
    }
}
