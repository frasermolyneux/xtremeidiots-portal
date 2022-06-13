using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IRecentPlayersApi
    {
        Task<ApiResponseDto<RecentPlayersCollectionDto>> GetRecentPlayers(GameType? gameType, Guid? gameServerId, DateTime? cutoff, RecentPlayersFilter? filter, int skipEntries, int takeEntries, RecentPlayersOrder? order);

        Task<ApiResponseDto> CreateRecentPlayers(List<CreateRecentPlayerDto> createRecentPlayerDtos);
    }
}