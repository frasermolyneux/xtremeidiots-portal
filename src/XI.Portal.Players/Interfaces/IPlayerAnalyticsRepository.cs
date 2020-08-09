using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Players.Dto;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayerAnalyticsRepository
    {
        Task<List<PlayerAnalyticEntryDto>> GetCumulativeDailyPlayers(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetNewDailyPlayersPerGame(DateTime cutoff);
        Task<List<PlayerAnalyticPerGameEntryDto>> GetPlayersDropOffPerGameJson(DateTime cutoff);
    }
}