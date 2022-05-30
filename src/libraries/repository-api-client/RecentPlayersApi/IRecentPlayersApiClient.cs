using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;

namespace XtremeIdiots.Portal.RepositoryApiClient.RecentPlayersApi
{
    public interface IRecentPlayersApiClient
    {
        Task<RecentPlayersCollectionDto?> GetRecentPlayers(GameType? gameType, Guid? serverId, DateTime? cutoff, RecentPlayersFilter? filterType, int skipEntries, int takeEntries, RecentPlayersOrder? order);
        Task CreateRecentPlayers(List<CreateRecentPlayerDto> createRecentPlayerDtos);
    }
}