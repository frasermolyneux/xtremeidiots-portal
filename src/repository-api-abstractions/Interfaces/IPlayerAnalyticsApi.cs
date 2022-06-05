using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IPlayerAnalyticsApi
    {
        Task<ApiResponseDto<PlayerAnalyticEntryCollectionDto>> GetCumulativeDailyPlayers(DateTime cutoff);
        Task<ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>> GetNewDailyPlayersPerGame(DateTime cutoff);
        Task<ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>> GetPlayersDropOffPerGameJson(DateTime cutoff);
    }
}
