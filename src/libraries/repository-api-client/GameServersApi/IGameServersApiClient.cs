using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi
{
    public interface IGameServersApiClient
    {
        Task<List<GameServerDto>?> GetGameServers(GameType[] gameTypes, Guid[] serverIds, string filterOption, int skipEntries, int takeEntries, string order);
        Task<GameServerDto?> GetGameServer(Guid serverId);
        Task CreateGameServer(GameServerDto gameServer);
        Task UpdateGameServer(GameServerDto gameServer);

        // Ban File Monitor Child Resources
        Task<BanFileMonitorDto> CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor);
        Task DeleteGameServer(Guid id);
    }
}