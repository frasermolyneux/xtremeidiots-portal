using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Repository
{
    public class PlayerAnalyticsRepository : IPlayerAnalyticsRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public PlayerAnalyticsRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<List<PlayerAnalyticEntryDto>> GetCumulativeDailyPlayers(DateTime cutoff)
        {
            var cumulative = await _legacyContext.Player2.CountAsync(p => p.FirstSeen < cutoff);

            var players = await _legacyContext.Player2
                .Where(p => p.FirstSeen > cutoff)
                .Select(p => p.FirstSeen)
                .OrderBy(p => p)
                .ToListAsync();

            var groupedPlayers = players.GroupBy(p => new DateTime(p.Year, p.Month, p.Day))
                .Select(g => new PlayerAnalyticEntryDto
                {
                    Created = g.Key,
                    Count = cumulative += g.Count()
                })
                .ToList();

            return groupedPlayers;
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>> GetNewDailyPlayersPerGame(DateTime cutoff)
        {
            var players = await _legacyContext.Player2
                .Where(p => p.FirstSeen > cutoff)
                .Select(p => new { p.FirstSeen, p.GameType })
                .OrderBy(p => p.FirstSeen)
                .ToListAsync();

            var groupedPlayers = players.GroupBy(p => new DateTime(p.FirstSeen.Year, p.FirstSeen.Month, p.FirstSeen.Day))
                .Select(g => new PlayerAnalyticPerGameEntryDto
                {
                    Created = g.Key,
                    GameCounts = g.GroupBy(i => i.GameType.ToString())
                        .Select(i => new {Type = i.Key, Count = i.Count()})
                        .ToDictionary(a => a.Type, a => a.Count)
                }).ToList();

            return groupedPlayers;
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var players = await _legacyContext.Player2
                .Where(p => p.LastSeen > cutoff)
                .Select(p => new { p.LastSeen, p.GameType })
                .OrderBy(p => p.LastSeen)
                .ToListAsync();

            var groupedPlayers = players.GroupBy(p => new DateTime(p.LastSeen.Year, p.LastSeen.Month, p.LastSeen.Day))
                .Select(g => new PlayerAnalyticPerGameEntryDto
                {
                    Created = g.Key,
                    GameCounts = g.GroupBy(i => i.GameType.ToString())
                        .Select(i => new {Type = i.Key, Count = i.Count()})
                        .ToDictionary(a => a.Type, a => a.Count)
                }).ToList();

            return groupedPlayers;
        }
    }
}