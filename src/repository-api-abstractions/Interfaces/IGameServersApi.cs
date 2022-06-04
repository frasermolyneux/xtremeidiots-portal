using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IGameServersApi
    {
        Task<List<GameServerDto>?> GetGameServers(GameType[] gameTypes, Guid[]? serverIds, GameServerFilter? filterOption, int skipEntries, int takeEntries, GameServerOrder? order);
        Task<GameServerDto?> GetGameServer(Guid serverId);
        Task CreateGameServer(CreateGameServerDto createGameServerDto);
        Task CreateGameServers(List<CreateGameServerDto> createGameServerDtos);
        Task UpdateGameServer(GameServerDto gameServer);

        // Ban File Monitor Child Resources
        Task<BanFileMonitorDto?> CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor);
        Task DeleteGameServer(Guid id);
    }
}