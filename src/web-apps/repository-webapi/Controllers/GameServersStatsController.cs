using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class GameServersStatsController : Controller
    {
        public GameServersStatsController(PortalDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PortalDbContext Context { get; }

        [HttpPost]
        [Route("api/game-servers-stats")]
        public async Task<IActionResult> CreateGameServerStats()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<GameServerStatDto>? gameServerStatDtos;
            try
            {
                gameServerStatDtos = JsonConvert.DeserializeObject<List<GameServerStatDto>>(requestBody);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }

            if (gameServerStatDtos == null || !gameServerStatDtos.Any())
                return new BadRequestResult();

            List<GameServerStat> gameServerStats = new();

            foreach (var gameServerStatDto in gameServerStatDtos)
            {
                var lastStat = await Context.GameServerStats.Where(gss => gss.GameServerId == gameServerStatDto.GameServerId).OrderBy(gss => gss.Timestamp).LastOrDefaultAsync();

                if (lastStat == null || lastStat.PlayerCount != gameServerStatDto.PlayerCount || lastStat.MapName != gameServerStatDto.MapName)
                {
                    gameServerStats.Add(new GameServerStat
                    {
                        GameServerId = gameServerStatDto.GameServerId,
                        PlayerCount = gameServerStatDto.PlayerCount,
                        MapName = gameServerStatDto.MapName,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            await Context.GameServerStats.AddRangeAsync(gameServerStats);
            await Context.SaveChangesAsync();

            var result = gameServerStats.Select(gss => gss.ToDto());

            return new OkObjectResult(result);
        }

        [HttpGet]
        [Route("api/game-servers-stats/{serverId}")]
        public async Task<IActionResult> GetGameServerStatusStats(Guid serverId, DateTime cutoff)
        {
            var gameServerStats = await Context.GameServerStats
                .Where(gss => gss.GameServerId == serverId && gss.Timestamp >= cutoff)
                .OrderByDescending(gss => gss.Timestamp)
                .ToListAsync();

            var result = gameServerStats.Select(gss => gss.ToDto());

            return new OkObjectResult(result);
        }
    }
}
