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
        private readonly ILogger<GameServersStatsController> logger;
        private readonly PortalDbContext context;

        public GameServersStatsController(
            ILogger<GameServersStatsController> logger,
            PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

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
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (gameServerStatDtos == null || !gameServerStatDtos.Any())
                return new BadRequestResult();

            List<GameServerStat> gameServerStats = new();

            foreach (var gameServerStatDto in gameServerStatDtos)
            {
                var lastStat = await context.GameServerStats.Where(gss => gss.GameServerId == gameServerStatDto.GameServerId).OrderBy(gss => gss.Timestamp).LastOrDefaultAsync();

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

            await context.GameServerStats.AddRangeAsync(gameServerStats);
            await context.SaveChangesAsync();

            var result = gameServerStats.Select(gss => gss.ToDto());

            return new OkObjectResult(result);
        }

        [HttpGet]
        [Route("api/game-servers-stats/{serverId}")]
        public async Task<IActionResult> GetGameServerStatusStats(Guid serverId, DateTime cutoff)
        {
            var gameServerStats = await context.GameServerStats
                .Where(gss => gss.GameServerId == serverId && gss.Timestamp >= cutoff)
                .OrderBy(gss => gss.Timestamp)
                .ToListAsync();

            var result = gameServerStats.Select(gss => gss.ToDto());

            return new OkObjectResult(result);
        }
    }
}
