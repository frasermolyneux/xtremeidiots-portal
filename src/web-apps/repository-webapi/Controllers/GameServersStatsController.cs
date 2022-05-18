using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            var gameServerStats = gameServerStatDtos.Select(gameServerStat => new GameServerStat
            {
                GameServerId = gameServerStat.GameServerId,
                PlayerCount = gameServerStat.PlayerCount,
                MapName = gameServerStat.MapName,
                Timestamp = DateTime.UtcNow
            });

            await Context.GameServerStats.AddRangeAsync(gameServerStats);
            await Context.SaveChangesAsync();

            var result = gameServerStats.Select(gss => gss.ToDto());

            return new OkObjectResult(result);
        }
    }
}
