using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayerAnalyticsApi
{
    public interface IPlayerAnalyticsApiClient
    {
        Task<List<PlayerAnalyticEntryDto>> GetCumulativeDailyPlayers(string accessToken, DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetNewDailyPlayersPerGame(string accessToken, DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetPlayersDropOffPerGameJson(string accessToken, DateTime cutoff);
    }
}
