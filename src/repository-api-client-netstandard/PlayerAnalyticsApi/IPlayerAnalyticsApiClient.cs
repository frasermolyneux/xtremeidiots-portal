using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayerAnalyticsApi
{
    public interface IPlayerAnalyticsApiClient
    {
        Task<List<PlayerAnalyticEntryDto>> GetCumulativeDailyPlayers(string accessToken, DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetNewDailyPlayersPerGame(string accessToken, DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetPlayersDropOffPerGameJson(string accessToken, DateTime cutoff);
    }
}
