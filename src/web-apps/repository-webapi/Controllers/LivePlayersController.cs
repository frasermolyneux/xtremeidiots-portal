using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class LivePlayersController : Controller
    {
        private readonly ILogger<LivePlayersController> logger;
        private readonly PortalDbContext context;

        public LivePlayersController(
            ILogger<LivePlayersController> logger,
            PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("/api/live-players")]
        public async Task<IActionResult> GetLivePlayers(GameType? gameType, Guid? serverId, LivePlayerFilter? filter)
        {
            if (gameType == null)
                gameType = GameType.Unknown;

            var query = context.LivePlayers.AsQueryable();
            query = ApplySearchFilter(query, (GameType)gameType, null, null);
            var totalCount = await query.CountAsync();

            query = ApplySearchFilter(query, (GameType)gameType, serverId, filter);
            var filteredCount = await query.CountAsync();

            query = ApplySearchOrderAndLimits(query);
            var results = await query.ToListAsync();

            var entries = results.Select(lp => lp.ToDto()).ToList();

            var response = new LivePlayersResponseDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new OkObjectResult(response);
        }

        [HttpPost]
        [Route("api/live-players/{serverId}")]
        public async Task<IActionResult> CreateGameServerLivePlayers(Guid serverId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<LivePlayerDto>? livePlayerDtos;
            try
            {
                livePlayerDtos = JsonConvert.DeserializeObject<List<LivePlayerDto>>(requestBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (livePlayerDtos == null)
                return new BadRequestResult();

            await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.LivePlayers)}] WHERE [GameServer_ServerId] = '{serverId}'");

            var livePlayers = livePlayerDtos.Select(livePlayerDto => new LivePlayer
            {
                Id = livePlayerDto.Id,
                Name = livePlayerDto.Name,
                Score = livePlayerDto.Score,
                Ping = livePlayerDto.Ping,
                Num = livePlayerDto.Num,
                Rate = livePlayerDto.Rate,
                Team = livePlayerDto.Team,
                Time = livePlayerDto.Time,
                IpAddress = livePlayerDto.IpAddress,
                Lat = livePlayerDto.Lat,
                Long = livePlayerDto.Long,
                CountryCode = livePlayerDto.CountryCode,
                GameServerServerId = livePlayerDto.GameServerServerId
            });

            await context.LivePlayers.AddRangeAsync(livePlayers);
            await context.SaveChangesAsync();

            var result = livePlayers.Select(lp => lp.ToDto());

            return new OkObjectResult(result);
        }

        [HttpPut]
        [Route("api/live-players")]
        public async Task<IActionResult> UpsertGameServerLivePlayers()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<LivePlayerDto>? livePlayerDtos;
            try
            {
                livePlayerDtos = JsonConvert.DeserializeObject<List<LivePlayerDto>>(requestBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (livePlayerDtos == null || !livePlayerDtos.Any())
                return new BadRequestResult();

            var livePlayerIds = livePlayerDtos.Select(lp => lp.Id).ToArray();

            var livePlayers = await context.LivePlayers.Where(lp => livePlayerIds.Contains(lp.Id)).ToListAsync();
            foreach (var livePlayerDto in livePlayerDtos)
            {
                var livePlayer = livePlayers.SingleOrDefault(lp => lp.Id == livePlayerDto.Id);

                if (livePlayer == null)
                    return new BadRequestResult();

                livePlayer.Name = livePlayerDto.Name;
                livePlayer.Score = livePlayerDto.Score;
                livePlayer.Ping = livePlayerDto.Ping;
                livePlayer.Num = livePlayerDto.Num;
                livePlayer.Rate = livePlayerDto.Rate;
                livePlayer.Team = livePlayerDto.Team;
                livePlayer.Time = livePlayerDto.Time;
                livePlayer.IpAddress = livePlayerDto.IpAddress;
                livePlayer.Lat = livePlayerDto.Lat;
                livePlayer.Long = livePlayerDto.Long;
                livePlayer.CountryCode = livePlayerDto.CountryCode;
                livePlayer.GameServerServerId = livePlayerDto.GameServerServerId;
            }

            await context.SaveChangesAsync();

            var result = livePlayers.Select(lp => lp.ToDto());

            return new OkObjectResult(result);
        }

        private IQueryable<LivePlayer> ApplySearchFilter(IQueryable<LivePlayer> query, GameType gameType, Guid? serverId, LivePlayerFilter? filter)
        {
            if (gameType != GameType.Unknown)
                query = query.Where(lp => lp.GameType == gameType.ToGameTypeInt()).AsQueryable();

            if (serverId != null)
                query = query.Where(lp => lp.GameServerServerId == serverId).AsQueryable();

            if (filter != null)
            {
                switch (filter)
                {
                    case LivePlayerFilter.GeoLocated:
                        query = query.Where(lp => lp.Lat != null && lp.Long != null).AsQueryable();
                        break;
                }
            }

            return query;
        }

        private IQueryable<LivePlayer> ApplySearchOrderAndLimits(IQueryable<LivePlayer> query)
        {
            query = query.OrderByDescending(lp => lp.Score).AsQueryable();

            return query;
        }
    }
}
