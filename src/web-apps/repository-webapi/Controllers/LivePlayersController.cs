using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class LivePlayersController : Controller, ILivePlayersApi
    {
        private readonly PortalDbContext context;
        private readonly IMapper mapper;

        public LivePlayersController(
            PortalDbContext context,
            IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [Route("/repository/live-players")]
        public async Task<IActionResult> GetLivePlayers(GameType? gameType, Guid? serverId, LivePlayerFilter? filter, int? skipEntries, int? takeEntries, LivePlayersOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            var response = await ((ILivePlayersApi)this).GetLivePlayers(gameType, serverId, filter, skipEntries.Value, takeEntries.Value, order);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<LivePlayersCollectionDto>> ILivePlayersApi.GetLivePlayers(GameType? gameType, Guid? serverId, LivePlayerFilter? filter, int skipEntries, int takeEntries, LivePlayersOrder? order)
        {
            var query = context.LivePlayers.Include(lp => lp.Player).AsQueryable();
            query = ApplyFilter(query, gameType, null, null);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, gameType, serverId, filter);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(lp => mapper.Map<LivePlayerDto>(lp)).ToList();

            var result = new LivePlayersCollectionDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new ApiResponseDto<LivePlayersCollectionDto>(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("repository/live-players/{serverId}")]
        public async Task<IActionResult> SetLivePlayersForGameServer(Guid serverId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateLivePlayerDto>? createLivePlayerDtos;
            try
            {
                createLivePlayerDtos = JsonConvert.DeserializeObject<List<CreateLivePlayerDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createLivePlayerDtos == null)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

            var response = await ((ILivePlayersApi)this).SetLivePlayersForGameServer(serverId, createLivePlayerDtos);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> ILivePlayersApi.SetLivePlayersForGameServer(Guid serverId, List<CreateLivePlayerDto> createLivePlayerDtos)
        {
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.LivePlayers)}] WHERE [GameServer_ServerId] = '{serverId}'");

            var livePlayers = createLivePlayerDtos.Select(lp => mapper.Map<LivePlayer>(lp)).ToList();

            await context.LivePlayers.AddRangeAsync(livePlayers);
            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        private IQueryable<LivePlayer> ApplyFilter(IQueryable<LivePlayer> query, GameType? gameType, Guid? serverId, LivePlayerFilter? filter)
        {
            if (gameType.HasValue)
                query = query.Where(lp => lp.GameType == gameType.Value.ToGameTypeInt()).AsQueryable();

            if (serverId.HasValue)
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

        private IQueryable<LivePlayer> ApplyOrderAndLimits(IQueryable<LivePlayer> query, int skipEntries, int takeEntries, LivePlayersOrder? order)
        {
            if (order.HasValue)
            {
                switch (order)
                {
                    case LivePlayersOrder.ScoreAsc:
                        query = query.OrderBy(rp => rp.Score).AsQueryable();
                        break;
                    case LivePlayersOrder.ScoreDesc:
                        query = query.OrderByDescending(rp => rp.Score).AsQueryable();
                        break;
                }
            }

            query = query.Skip(skipEntries).AsQueryable();
            query = query.Take(takeEntries).AsQueryable();

            return query;
        }
    }
}
