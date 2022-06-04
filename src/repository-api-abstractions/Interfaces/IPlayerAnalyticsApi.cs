using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces
{
    public interface IPlayerAnalyticsApi
    {
        Task<List<PlayerAnalyticEntryDto>?> GetCumulativeDailyPlayers(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>?> GetNewDailyPlayersPerGame(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>?> GetPlayersDropOffPerGameJson(DateTime cutoff);
    }
}
