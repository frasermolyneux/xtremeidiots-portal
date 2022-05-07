using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class PlayerAnalyticsController : Controller
    {
        public PlayerAnalyticsController(PortalDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PortalDbContext Context { get; }

        [HttpGet]
        [Route("api/player-analytics/cumulative-daily-players")]
        public async Task<IActionResult> GetCumulativeDailyPlayers(DateTime cutoff)
        {
            var cumulative = await Context.Player2s.CountAsync(p => p.FirstSeen < cutoff);

            var players = await Context.Player2s
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

            return new OkObjectResult(groupedPlayers);
        }

        [HttpGet]
        [Route("api/player-analytics/new-daily-players-per-game")]
        public async Task<IActionResult> GetNewDailyPlayersPerGame(DateTime cutoff)
        {
            var players = await Context.Player2s
                .Where(p => p.FirstSeen > cutoff)
                .Select(p => new { p.FirstSeen, p.GameType })
                .OrderBy(p => p.FirstSeen)
                .ToListAsync();

            var groupedPlayers = players.GroupBy(p => new DateTime(p.FirstSeen.Year, p.FirstSeen.Month, p.FirstSeen.Day))
                .Select(g => new PlayerAnalyticPerGameEntryDto
                {
                    Created = g.Key,
                    GameCounts = g.GroupBy(i => i.GameType.ToString())
                        .Select(i => new { Type = i.Key, Count = i.Count() })
                        .ToDictionary(a => a.Type, a => a.Count)
                }).ToList();

            return new OkObjectResult(groupedPlayers);
        }

        [HttpGet]
        [Route("api/player-analytics/players-drop-off-per-game")]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var players = await Context.Player2s
                .Where(p => p.LastSeen > cutoff)
                .Select(p => new { p.LastSeen, p.GameType })
                .OrderBy(p => p.LastSeen)
                .ToListAsync();

            var groupedPlayers = players.GroupBy(p => new DateTime(p.LastSeen.Year, p.LastSeen.Month, p.LastSeen.Day))
                .Select(g => new PlayerAnalyticPerGameEntryDto
                {
                    Created = g.Key,
                    GameCounts = g.GroupBy(i => i.GameType.ToString())
                        .Select(i => new { Type = i.Key, Count = i.Count() })
                        .ToDictionary(a => a.Type, a => a.Count)
                }).ToList();

            return new OkObjectResult(groupedPlayers);
        }
    }
}
