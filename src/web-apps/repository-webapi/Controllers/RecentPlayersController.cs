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
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class RecentPlayersController : ControllerBase, IRecentPlayersApi
    {
        private readonly ILogger<RecentPlayersController> logger;
        private readonly PortalDbContext context;
        private readonly IMapper mapper;

        public RecentPlayersController(
            ILogger<RecentPlayersController> logger,
            PortalDbContext context,
            IMapper mapper)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [Route("api/recent-players")]
        public async Task<IActionResult> GetRecentPlayersApi(GameType? gameType, Guid? serverId, DateTime? cutoff, RecentPlayersFilter? filterType, int? skipEntries, int? takeEntries, RecentPlayersOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            if (cutoff.HasValue && cutoff.Value < DateTime.UtcNow.AddHours(-48))
                cutoff = DateTime.UtcNow.AddHours(-48);

            var response = await GetRecentPlayers(gameType, serverId, cutoff, filterType, skipEntries.Value, takeEntries.Value, order);

            return new OkObjectResult(response);
        }

        public async Task<ApiResponseDto<RecentPlayersCollectionDto>> GetRecentPlayers(GameType? gameType, Guid? serverId, DateTime? cutoff, RecentPlayersFilter? filterType, int skipEntries, int takeEntries, RecentPlayersOrder? order)
        {
            var query = context.RecentPlayers.AsQueryable();
            query = ApplyFilter(query, gameType, null, null, null);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, gameType, serverId, cutoff, filterType);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(rp => mapper.Map<RecentPlayerDto>(rp)).ToList();

            var result = new RecentPlayersCollectionDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new ApiResponseDto<RecentPlayersCollectionDto>(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("api/recent-players")]
        public async Task<IActionResult> CreateRecentPlayersApi()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateRecentPlayerDto>? createRecentPlayersDto;
            try
            {
                createRecentPlayersDto = JsonConvert.DeserializeObject<List<CreateRecentPlayerDto>>(requestBody);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (createRecentPlayersDto == null || !createRecentPlayersDto.Any())
            {
                logger.LogWarning("Request body was null or did not contain any entries");
                return new BadRequestResult();
            }

            var response = await CreateRecentPlayers(createRecentPlayersDto);

            return new OkObjectResult(response);
        }

        public async Task<ApiResponseDto> CreateRecentPlayers(List<CreateRecentPlayerDto> createRecentPlayerDtos)
        {
            foreach (var createRecentPlayerDto in createRecentPlayerDtos)
            {
                var recentPlayer = await context.RecentPlayers.SingleOrDefaultAsync(rp => rp.PlayerId == createRecentPlayerDto.PlayerId);

                if (recentPlayer != null)
                {
                    mapper.Map(createRecentPlayerDto, recentPlayer);
                    recentPlayer.Timestamp = DateTime.UtcNow;
                }
                else
                {
                    recentPlayer = mapper.Map<RecentPlayer>(createRecentPlayerDto);
                    recentPlayer.Timestamp = DateTime.UtcNow;

                    await context.RecentPlayers.AddAsync(recentPlayer);
                }
            }

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        private static IQueryable<RecentPlayer> ApplyFilter(IQueryable<RecentPlayer> query, GameType? gameType, Guid? serverId, DateTime? cutoff, RecentPlayersFilter? filterType)
        {
            if (gameType.HasValue)
                query = query.Where(rp => rp.GameType == gameType.Value.ToGameTypeInt()).AsQueryable();

            if (serverId.HasValue)
                query = query.Where(rp => rp.ServerId == serverId).AsQueryable();

            if (cutoff.HasValue)
                query = query.Where(rp => rp.Timestamp > cutoff).AsQueryable();

            if (filterType.HasValue)
            {
                switch (filterType)
                {
                    case RecentPlayersFilter.GeoLocated:
                        query = query.Where(rp => rp.Lat != 0 && rp.Long != 0).AsQueryable();
                        break;
                }
            }

            return query;
        }

        private static IQueryable<RecentPlayer> ApplyOrderAndLimits(IQueryable<RecentPlayer> query, int skipEntries, int takeEntries, RecentPlayersOrder? order)
        {
            query = query.Skip(skipEntries).AsQueryable();
            query = query.Take(takeEntries).AsQueryable();

            if (order.HasValue)
            {
                switch (order)
                {
                    case RecentPlayersOrder.TimestampAsc:
                        query = query.OrderBy(rp => rp.Timestamp).AsQueryable();
                        break;
                    case RecentPlayersOrder.TimestampDesc:
                        query = query.OrderByDescending(rp => rp.Timestamp).AsQueryable();
                        break;
                }
            }

            return query;
        }
    }
}
