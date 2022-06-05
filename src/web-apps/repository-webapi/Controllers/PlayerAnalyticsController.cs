using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class PlayerAnalyticsController : Controller, IPlayerAnalyticsApi
    {
        private readonly PortalDbContext context;

        public PlayerAnalyticsController(PortalDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [Route("api/player-analytics/cumulative-daily-players")]
        public async Task<IActionResult> GetCumulativeDailyPlayers(DateTime cutoff)
        {
            var response = await ((IPlayerAnalyticsApi)this).GetCumulativeDailyPlayers(cutoff);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<PlayerAnalyticEntryCollectionDto>> IPlayerAnalyticsApi.GetCumulativeDailyPlayers(DateTime cutoff)
        {
            var cumulative = await context.Players.CountAsync(p => p.FirstSeen < cutoff);

            var players = await context.Players
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

            var result = new PlayerAnalyticEntryCollectionDto
            {
                TotalRecords = groupedPlayers.Count,
                FilteredRecords = groupedPlayers.Count,
                Entries = groupedPlayers
            };

            return new ApiResponseDto<PlayerAnalyticEntryCollectionDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("api/player-analytics/new-daily-players-per-game")]
        public async Task<IActionResult> GetNewDailyPlayersPerGame(DateTime cutoff)
        {
            var response = await ((IPlayerAnalyticsApi)this).GetNewDailyPlayersPerGame(cutoff);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>> IPlayerAnalyticsApi.GetNewDailyPlayersPerGame(DateTime cutoff)
        {
            var players = await context.Players
                .Where(p => p.FirstSeen > cutoff)
                .Select(p => new { p.FirstSeen, p.GameType })
                .OrderBy(p => p.FirstSeen)
                .ToListAsync();

            var groupedPlayers = players.GroupBy(p => new DateTime(p.FirstSeen.Year, p.FirstSeen.Month, p.FirstSeen.Day))
                .Select(g => new PlayerAnalyticPerGameEntryDto
                {
                    Created = g.Key,
                    GameCounts = g.GroupBy(i => i.GameType)
                        .Select(i => new { Type = i.Key, Count = i.Count() })
                        .ToDictionary(a => a.Type.ToGameType(), a => a.Count)
                }).ToList();

            var result = new PlayerAnalyticPerGameEntryCollectionDto
            {
                TotalRecords = groupedPlayers.Count,
                FilteredRecords = groupedPlayers.Count,
                Entries = groupedPlayers
            };

            return new ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("api/player-analytics/players-drop-off-per-game")]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var response = await ((IPlayerAnalyticsApi)this).GetPlayersDropOffPerGameJson(cutoff);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>> IPlayerAnalyticsApi.GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var players = await context.Players
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
                        .ToDictionary(a => a.Type.ToGameType(), a => a.Count)
                }).ToList();

            var result = new PlayerAnalyticPerGameEntryCollectionDto
            {
                TotalRecords = groupedPlayers.Count,
                FilteredRecords = groupedPlayers.Count,
                Entries = groupedPlayers
            };

            return new ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>(HttpStatusCode.OK, result);
        }
    }
}
