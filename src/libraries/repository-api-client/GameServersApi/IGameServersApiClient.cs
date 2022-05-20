using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi
{
    public interface IGameServersApiClient
    {
        Task<List<GameServerDto>?> GetGameServers(GameType[] gameTypes, Guid[] serverIds, GameServerFilter? filterOption, int skipEntries, int takeEntries, GameServerOrder? order);
        Task<GameServerDto?> GetGameServer(Guid serverId);
        Task CreateGameServer(GameServerDto gameServer);
        Task UpdateGameServer(GameServerDto gameServer);

        // Ban File Monitor Child Resources
        Task<BanFileMonitorDto?> CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor);
        Task DeleteGameServer(Guid id);
    }
}