using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class RecentPlayersController : ControllerBase
    {
        private readonly ILogger<RecentPlayersController> logger;
        private readonly PortalDbContext context;

        public RecentPlayersController(
            ILogger<RecentPlayersController> logger,
            PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("api/recent-players")]
        public async Task<IActionResult> GetRecentPlayers(GameType? gameType, Guid? serverId, DateTime? cutoff, RecentPlayersFilter? filterType, int? skipEntries, int? takeEntries, RecentPlayersOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            if (cutoff.HasValue && cutoff.Value < DateTime.UtcNow.AddHours(-48))
                cutoff = DateTime.UtcNow.AddHours(-48);

            var query = context.RecentPlayers.AsQueryable();
            query = ApplyFilter(query, gameType, null, null, null);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, gameType, serverId, cutoff, filterType);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, (int)skipEntries, (int)takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(rp => new RecentPlayerDtoWrapper(rp)).ToList();

            var response = new CollectionResponseDto<RecentPlayerDtoWrapper>
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new OkObjectResult(response);
        }

        [HttpPost]
        [Route("api/recent-players")]
        public async Task<IActionResult> CreateRecentPlayers()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateRecentPlayerDto>? createRecentPlayersDto;
            try
            {
                createRecentPlayersDto = JsonConvert.DeserializeObject<List<CreateRecentPlayerDto>>(requestBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (createRecentPlayersDto == null || createRecentPlayersDto.Count == 0)
                return new BadRequestResult();

            foreach (var createRecentPlayerDto in createRecentPlayersDto)
            {
                var recentPlayer = await context.RecentPlayers.SingleOrDefaultAsync(rp => rp.PlayerId == createRecentPlayerDto.PlayerId);

                if (recentPlayer != null)
                {
                    recentPlayer.Name = createRecentPlayerDto.Name ?? recentPlayer.Name;
                    recentPlayer.IpAddress = createRecentPlayerDto.IpAddress ?? recentPlayer.IpAddress;
                    recentPlayer.Lat = createRecentPlayerDto.Lat != 0 ? createRecentPlayerDto.Lat : recentPlayer.Lat;
                    recentPlayer.Long = createRecentPlayerDto.Long != 0 ? createRecentPlayerDto.Long : recentPlayer.Long;
                    recentPlayer.CountryCode = createRecentPlayerDto.CountryCode ?? recentPlayer.CountryCode;
                    recentPlayer.Timestamp = DateTime.UtcNow;
                }
                else
                {
                    recentPlayer = new RecentPlayer
                    {
                        Name = createRecentPlayerDto.Name,
                        IpAddress = createRecentPlayerDto.IpAddress,
                        Lat = createRecentPlayerDto.Lat,
                        Long = createRecentPlayerDto.Long,
                        CountryCode = createRecentPlayerDto.CountryCode,
                        GameType = createRecentPlayerDto.GameType.ToGameTypeInt(),
                        PlayerId = createRecentPlayerDto.PlayerId,
                        ServerId = createRecentPlayerDto.ServerId,
                        Timestamp = DateTime.UtcNow
                    };

                    await context.RecentPlayers.AddAsync(recentPlayer);
                }
            }

            await context.SaveChangesAsync();

            return new OkResult();
        }

        private static IQueryable<RecentPlayer> ApplyFilter(IQueryable<RecentPlayer> query, GameType? gameType, Guid? serverId, DateTime? cutoff, RecentPlayersFilter? filterType)
        {
            if (gameType.HasValue)
                query = query.Where(rp => rp.GameType == ((GameType)gameType).ToGameTypeInt()).AsQueryable();

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

        public class RecentPlayerDtoWrapper : RecentPlayerDto
        {
            public RecentPlayerDtoWrapper(RecentPlayer recentPlayer)
            {
                Id = recentPlayer.Id;
                Name = recentPlayer.Name;
                IpAddress = recentPlayer.IpAddress;
                Lat = recentPlayer.Lat;
                Long = recentPlayer.Long;
                CountryCode = recentPlayer.CountryCode;
                GameType = recentPlayer.GameType.ToGameType();
                PlayerId = recentPlayer.PlayerId;
                ServerId = recentPlayer.ServerId;
            }
        }
    }
}
