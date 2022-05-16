using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayerAnalyticsApi
{
    public interface IPlayerAnalyticsApiClient
    {
        Task<List<PlayerAnalyticEntryDto>> GetCumulativeDailyPlayers(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetNewDailyPlayersPerGame(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetPlayersDropOffPerGameJson(DateTime cutoff);
    }
}
