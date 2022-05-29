using Azure.Storage.Blobs;
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
    public class MapsController : Controller
    {
        private readonly ILogger<MapsController> logger;
        private readonly PortalDbContext context;

        public MapsController(
            ILogger<MapsController> logger,
            PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpPost]
        [Route("api/maps")]
        public async Task<IActionResult> CreateMaps()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<MapDto>? mapDtos;
            try
            {
                mapDtos = JsonConvert.DeserializeObject<List<MapDto>>(requestBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (mapDtos == null || mapDtos.Count == 0)
                return new BadRequestResult();

            var maps = mapDtos.Select(mapDto => new Map()
            {
                GameType = mapDto.GameType.ToGameTypeInt(),
                MapName = mapDto.MapName,
                MapFiles = JsonConvert.SerializeObject(mapDto.MapFiles),
            });

            var conflict = maps.Any(m => context.Maps.Any(mm => mm.GameType == m.GameType && mm.MapName == m.MapName));
            if (conflict)
                return new ConflictResult();

            await context.Maps.AddRangeAsync(maps);
            await context.SaveChangesAsync();

            var result = maps.Select(m => m.ToDto());

            return new OkObjectResult(result);
        }

        [HttpDelete]
        [Route("api/maps/{mapId}")]
        public async Task<IActionResult> DeleteMap(Guid mapId)
        {
            var map = await context.Maps
                .SingleOrDefaultAsync(m => m.MapId == mapId);

            if (map == null)
                return NotFound();

            context.Remove(map);
            await context.SaveChangesAsync();

            return new OkResult();
        }

        [HttpGet]
        [Route("/api/maps/{mapId}")]
        public async Task<IActionResult> GetMap(Guid mapId)
        {
            var map = await context.Maps
                .SingleOrDefaultAsync(m => m.MapId == mapId);

            if (map == null)
                return NotFound();

            return new OkObjectResult(map.ToDto());
        }

        [HttpGet]
        [Route("/api/maps/{gameType}/{mapName}")]
        public async Task<IActionResult> GetMap(GameType gameType, string mapName)
        {
            var map = await context.Maps
                .SingleOrDefaultAsync(m => m.GameType == gameType.ToGameTypeInt() && m.MapName == mapName);

            if (map == null)
                return NotFound();

            return new OkObjectResult(map.ToDto());
        }

        [HttpGet]
        [Route("/api/maps")]
        public async Task<IActionResult> GetMaps(GameType? gameType, string? mapNames, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order)
        {
            if (gameType == null)
                gameType = GameType.Unknown;

            if (order == null)
                order = MapsOrder.MapNameDesc;

            if (filterString == null)
                filterString = string.Empty;

            if (skipEntries == null)
                skipEntries = 0;

            if (takeEntries == null)
                takeEntries = 0;

            string[] mapNamesFilter = new string[] { };
            if (!string.IsNullOrWhiteSpace(mapNames))
            {
                var split = mapNames.Split(",");
                mapNamesFilter = split.Select(mn => mn.Trim()).ToArray();
            }

            var query = context.Maps.AsQueryable();
            query = ApplySearchFilter(query, (GameType)gameType, null, null);
            var totalCount = await query.CountAsync();

            query = ApplySearchFilter(query, (GameType)gameType, mapNames != null ? mapNamesFilter : null, filterString);
            var filteredCount = await query.CountAsync();

            query = ApplySearchOrderAndLimits(query, (MapsOrder)order, (int)skipEntries, (int)takeEntries);
            var searchResults = await query.ToListAsync();

            var entries = searchResults.Select(m => m.ToDto()).ToList();

            var response = new MapsResponseDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new OkObjectResult(response);
        }

        [HttpPut]
        [Route("api/maps")]
        public async Task<IActionResult> UpdateMaps()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<MapDto>? mapDtos;
            try
            {
                mapDtos = JsonConvert.DeserializeObject<List<MapDto>>(requestBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (mapDtos == null || mapDtos.Count == 0)
                return new BadRequestResult();

            var mapIds = mapDtos.Select(m => m.MapId).ToArray();

            var maps = await context.Maps.Where(m => mapIds.Contains(m.MapId)).ToListAsync();
            foreach (var mapDto in mapDtos)
            {
                var map = maps.SingleOrDefault(m => m.MapId == mapDto.MapId);

                if (map == null)
                    return new BadRequestResult();

                map.MapFiles = JsonConvert.SerializeObject(mapDto.MapFiles);
            }

            await context.SaveChangesAsync();

            var result = maps.Select(m => m.ToDto());

            return new OkObjectResult(result);
        }

        [HttpPost]
        [Route("api/maps/popularity")]
        public async Task<IActionResult> RebuildMapPopularity()
        {
            var maps = await context.Maps.Include(m => m.MapVotes).ToListAsync();

            foreach (var map in maps)
            {
                map.TotalLikes = map.MapVotes.Count(mv => mv.Like);
                map.TotalDislikes = map.MapVotes.Count(mv => !mv.Like);
                map.TotalVotes = map.MapVotes.Count;

                if (map.TotalVotes > 0)
                {
                    map.LikePercentage = (double)map.TotalLikes / map.TotalVotes * 100;
                    map.DislikePercentage = (double)map.TotalDislikes / map.TotalVotes * 100;
                }
                else
                {
                    map.LikePercentage = 0;
                    map.DislikePercentage = 0;
                }
            }

            await context.SaveChangesAsync();
            return new OkResult();
        }

        [HttpPost]
        [Route("/api/maps/{mapId}/popularity/{playerId}")]
        public async Task<IActionResult> UpsertMapVote(Guid mapId, Guid playerId, bool like, DateTime? overrideCreated)
        {
            var mapVote = await context.MapVotes
                .SingleOrDefaultAsync(mv => mv.MapMapId == mapId && mv.PlayerPlayerId == playerId);

            if (mapVote == null)
            {
                var mapVoteToAdd = new MapVote
                {
                    MapMapId = mapId,
                    PlayerPlayerId = playerId,
                    Like = like,
                    Timestamp = DateTime.UtcNow
                };

                if (overrideCreated != null)
                    mapVoteToAdd.Timestamp = (DateTime)overrideCreated;

                context.MapVotes.Add(mapVoteToAdd);
            }
            else
            {
                if (mapVote.Like != like)
                {
                    mapVote.Like = like;
                    mapVote.Timestamp = DateTime.UtcNow;
                }

                if (overrideCreated != null)
                    mapVote.Timestamp = (DateTime)overrideCreated;
            }

            await context.SaveChangesAsync();

            return new OkResult();
        }

        [HttpPost]
        [Route("/api/maps/{mapId}/image")]
        public async Task<IActionResult> UpdateMapImage(Guid mapId)
        {
            var map = await context.Maps
                .SingleOrDefaultAsync(m => m.MapId == mapId);

            if (map == null)
                return new NotFoundResult();

            if (Request.Form.Files.Count == 0)
                return new BadRequestResult();

            var file = Request.Form.Files.First();

            var blobKey = $"{map.GameType.ToGameType()}_{map.MapName}.jpg";
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("appdata-storage-connectionstring"));
            var containerClient = blobServiceClient.GetBlobContainerClient("map-images");

            var blobClient = containerClient.GetBlobClient(blobKey);
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
            }

            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            await blobClient.UploadAsync(filePath);

            map.MapImageUri = blobClient.Uri.ToString();

            await context.SaveChangesAsync();

            return new OkResult();
        }

        private IQueryable<Map> ApplySearchFilter(IQueryable<Map> maps, GameType gameType, string[]? mapNames, string? filterString)
        {
            maps = maps.AsQueryable();

            if (gameType != GameType.Unknown) maps = maps.Where(m => m.GameType == gameType.ToGameTypeInt()).AsQueryable();

            if (mapNames != null && mapNames.Length > 0) maps = maps.Where(m => mapNames.Contains(m.MapName)).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterString)) maps = maps.Where(m => m.MapName.Contains(filterString)).AsQueryable();

            return maps;
        }

        private IQueryable<Map> ApplySearchOrderAndLimits(IQueryable<Map> maps, MapsOrder order, int skipEntries, int takeEntries)
        {
            switch (order)
            {
                case MapsOrder.MapNameAsc:
                    maps = maps.OrderBy(m => m.MapName).AsQueryable();
                    break;
                case MapsOrder.MapNameDesc:
                    maps = maps.OrderByDescending(m => m.MapName).AsQueryable();
                    break;
                case MapsOrder.GameTypeAsc:
                    maps = maps.OrderBy(m => m.GameType).AsQueryable();
                    break;
                case MapsOrder.GameTypeDesc:
                    maps = maps.OrderByDescending(m => m.GameType).AsQueryable();
                    break;
                case MapsOrder.PopularityAsc:
                    maps = maps.OrderByDescending(m => m.TotalLikes).AsQueryable();
                    break;
                case MapsOrder.PopularityDesc:
                    maps = maps.OrderByDescending(m => m.TotalDislikes).AsQueryable();
                    break;
            }

            maps = maps.Skip(skipEntries).AsQueryable();

            if (takeEntries != 0) maps = maps.Take(takeEntries).AsQueryable();

            return maps;
        }
    }
}
