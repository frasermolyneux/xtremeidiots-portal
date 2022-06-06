
using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class GameServersStatsController : Controller, IGameServersStatsApi
    {
        private readonly PortalDbContext context;
        private readonly IMapper mapper;

        public GameServersStatsController(
            PortalDbContext context,
            IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost]
        [Route("api/game-servers-stats")]
        public async Task<IActionResult> CreateGameServerStats()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateGameServerStatDto>? createGameServerStatDto;
            try
            {
                createGameServerStatDto = JsonConvert.DeserializeObject<List<CreateGameServerStatDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createGameServerStatDto == null || !createGameServerStatDto.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IGameServersStatsApi)this).CreateGameServerStats(createGameServerStatDto);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IGameServersStatsApi.CreateGameServerStats(List<CreateGameServerStatDto> createGameServerStatDtos)
        {
            var gameServerStats = new List<GameServerStat>();

            foreach (var createGameServerStatDto in createGameServerStatDtos)
            {
                var lastStat = await context.GameServerStats.Where(gss => gss.GameServerId == createGameServerStatDto.GameServerId).OrderBy(gss => gss.Timestamp).LastOrDefaultAsync();

                if (lastStat == null || lastStat.PlayerCount != createGameServerStatDto.PlayerCount || lastStat.MapName != createGameServerStatDto.MapName)
                {
                    var gameServerStat = mapper.Map<GameServerStat>(createGameServerStatDto);
                    gameServerStat.Timestamp = DateTime.UtcNow;

                    gameServerStats.Add(gameServerStat);
                }
            }

            await context.GameServerStats.AddRangeAsync(gameServerStats);
            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/game-servers-stats/{serverId}")]
        public async Task<IActionResult> GetGameServerStatusStats(Guid serverId, DateTime? cutoff)
        {
            if (!cutoff.HasValue)
                cutoff = DateTime.UtcNow.AddDays(-2);

            if (cutoff.HasValue && cutoff.Value < DateTime.UtcNow.AddDays(-2))
                cutoff = DateTime.UtcNow.AddDays(-2);

            var response = await ((IGameServersStatsApi)this).GetGameServerStatusStats(serverId, cutoff.Value);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<GameServerStatCollectionDto>> IGameServersStatsApi.GetGameServerStatusStats(Guid serverId, DateTime cutoff)
        {
            var gameServerStats = await context.GameServerStats
                .Where(gss => gss.GameServerId == serverId && gss.Timestamp >= cutoff)
                .OrderBy(gss => gss.Timestamp)
                .ToListAsync();

            var entries = gameServerStats.Select(r => mapper.Map<GameServerStatDto>(r)).ToList();

            var result = new GameServerStatCollectionDto
            {
                TotalRecords = entries.Count,
                FilteredRecords = entries.Count,
                Entries = entries
            };

            return new ApiResponseDto<GameServerStatCollectionDto>(HttpStatusCode.OK, result);
        }
    }
}
