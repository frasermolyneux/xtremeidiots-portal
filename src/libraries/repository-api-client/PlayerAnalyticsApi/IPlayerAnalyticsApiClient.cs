using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayerAnalyticsApi
{
    public interface IPlayerAnalyticsApiClient
    {
        Task<List<PlayerAnalyticEntryDto>?> GetCumulativeDailyPlayers(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>?> GetNewDailyPlayersPerGame(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>?> GetPlayersDropOffPerGameJson(DateTime cutoff);
    }
}
