using AutoMapper;

using Azure.Storage.Blobs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class MapsController : Controller, IMapsApi
    {
        private readonly PortalDbContext context;
        private readonly IMapper mapper;

        public MapsController(
            PortalDbContext context,
            IMapper mapper)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [Route("maps/{mapId}")]
        public async Task<IActionResult> GetMap(Guid mapId)
        {
            var response = await ((IMapsApi)this).GetMap(mapId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<MapDto>> IMapsApi.GetMap(Guid mapId)
        {
            var map = await context.Maps
                .Include(m => m.MapVotes)
                .SingleOrDefaultAsync(m => m.MapId == mapId);

            if (map == null)
                return new ApiResponseDto<MapDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<MapDto>(map);

            return new ApiResponseDto<MapDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("maps/{gameType}/{mapName}")]
        public async Task<IActionResult> GetMap(GameType gameType, string mapName)
        {
            var response = await ((IMapsApi)this).GetMap(gameType, mapName);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<MapDto>> IMapsApi.GetMap(GameType gameType, string mapName)
        {
            var map = await context.Maps
                .Include(m => m.MapVotes)
                .SingleOrDefaultAsync(m => m.GameType == gameType.ToGameTypeInt() && m.MapName == mapName);

            if (map == null)
                return new ApiResponseDto<MapDto>(HttpStatusCode.NotFound);

            var result = mapper.Map<MapDto>(map);

            return new ApiResponseDto<MapDto>(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("maps")]
        public async Task<IActionResult> GetMaps(GameType? gameType, string? mapNames, MapsFilter? filter, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order)
        {
            if (!skipEntries.HasValue)
                skipEntries = 0;

            if (!takeEntries.HasValue)
                takeEntries = 20;

            string[]? mapNamesFilter = null;
            if (!string.IsNullOrWhiteSpace(mapNames))
            {
                var split = mapNames.Split(",");
                mapNamesFilter = split.Select(mn => mn.Trim()).ToArray();
            }

            var response = await ((IMapsApi)this).GetMaps(gameType, mapNamesFilter, filter, filterString, skipEntries.Value, takeEntries.Value, order);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<MapsCollectionDto>> IMapsApi.GetMaps(GameType? gameType, string[]? mapNames, MapsFilter? filter, string? filterString, int skipEntries, int takeEntries, MapsOrder? order)
        {
            var query = context.Maps.AsQueryable();
            query = ApplyFilter(query, gameType, null, null, null);
            var totalCount = await query.CountAsync();

            query = ApplyFilter(query, gameType, mapNames, filter, filterString);
            var filteredCount = await query.CountAsync();

            query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
            var results = await query.ToListAsync();

            var entries = results.Select(m => mapper.Map<MapDto>(m)).ToList();

            var result = new MapsCollectionDto
            {
                TotalRecords = totalCount,
                FilteredRecords = filteredCount,
                Entries = entries
            };

            return new ApiResponseDto<MapsCollectionDto>(HttpStatusCode.OK, result);
        }

        Task<ApiResponseDto> IMapsApi.CreateMap(CreateMapDto createMapDto)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("maps")]
        public async Task<IActionResult> CreateMaps()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<CreateMapDto>? createMapDtos;
            try
            {
                createMapDtos = JsonConvert.DeserializeObject<List<CreateMapDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (createMapDtos == null || !createMapDtos.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IMapsApi)this).CreateMaps(createMapDtos);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IMapsApi.CreateMaps(List<CreateMapDto> createMapDtos)
        {
            foreach (var createMapDto in createMapDtos)
            {
                if (await context.Maps.AnyAsync(m => m.GameType == createMapDto.GameType.ToGameTypeInt() && m.MapName == createMapDto.MapName))
                    return new ApiResponseDto(HttpStatusCode.Conflict, $"Map with gameType '{createMapDto.GameType}' and name '{createMapDto.MapName}' already exists");

                var map = mapper.Map<Map>(createMapDto);
                await context.Maps.AddAsync(map);
            }

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        Task<ApiResponseDto> IMapsApi.UpdateMap(EditMapDto editMapDto)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("maps")]
        public async Task<IActionResult> UpdateMaps()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<EditMapDto>? editMapDtos;
            try
            {
                editMapDtos = JsonConvert.DeserializeObject<List<EditMapDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (editMapDtos == null || !editMapDtos.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IMapsApi)this).UpdateMaps(editMapDtos);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IMapsApi.UpdateMaps(List<EditMapDto> editMapDtos)
        {
            foreach (var editMapDto in editMapDtos)
            {
                var map = await context.Maps.SingleAsync(m => m.MapId == editMapDto.MapId);
                mapper.Map(editMapDto, map);
            }

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("maps/{mapId}")]
        public async Task<IActionResult> DeleteMap(Guid mapId)
        {
            var response = await ((IMapsApi)this).DeleteMap(mapId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IMapsApi.DeleteMap(Guid mapId)
        {
            var map = await context.Maps
                .SingleOrDefaultAsync(m => m.MapId == mapId);

            if (map == null)
                return new ApiResponseDto(HttpStatusCode.NotFound);

            context.Remove(map);

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("maps/popularity")]
        public async Task<IActionResult> RebuildMapPopularity()
        {
            var response = await ((IMapsApi)this).RebuildMapPopularity();

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IMapsApi.RebuildMapPopularity()
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

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        Task<ApiResponseDto> IMapsApi.UpsertMapVote(UpsertMapVoteDto upsertMapVoteDto)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("maps/votes")]
        public async Task<IActionResult> UpsertMapVotes()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            List<UpsertMapVoteDto>? upsertMapVoteDtos;
            try
            {
                upsertMapVoteDtos = JsonConvert.DeserializeObject<List<UpsertMapVoteDto>>(requestBody);
            }
            catch
            {
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
            }

            if (upsertMapVoteDtos == null || !upsertMapVoteDtos.Any())
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

            var response = await ((IMapsApi)this).UpsertMapVotes(upsertMapVoteDtos);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IMapsApi.UpsertMapVotes(List<UpsertMapVoteDto> upsertMapVoteDtos)
        {
            foreach (var upsertMapVote in upsertMapVoteDtos)
            {
                var mapVote = await context.MapVotes.SingleOrDefaultAsync(mv => mv.MapId == upsertMapVote.MapId && mv.PlayerId == upsertMapVote.PlayerId && mv.GameServerId == upsertMapVote.GameServerId);

                if (mapVote == null)
                {
                    var newMapVote = mapper.Map<MapVote>(upsertMapVote);
                    newMapVote.Timestamp = DateTime.UtcNow;

                    context.MapVotes.Add(newMapVote);
                }
                else
                {
                    if (mapVote.Like != upsertMapVote.Like)
                    {
                        mapVote.Like = upsertMapVote.Like;
                        mapVote.Timestamp = DateTime.UtcNow;
                    }
                }
            }

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("maps/{mapId}/image")]
        public async Task<IActionResult> UpdateMapImage(Guid mapId)
        {
            if (Request.Form.Files.Count == 0)
                return new ApiResponseDto(HttpStatusCode.BadRequest, "Request does not contain any files").ToHttpResult();

            var file = Request.Form.Files.First();

            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var response = await ((IMapsApi)this).UpdateMapImage(mapId, filePath);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IMapsApi.UpdateMapImage(Guid mapId, string filePath)
        {
            var map = await context.Maps
                .SingleOrDefaultAsync(m => m.MapId == mapId);

            if (map == null)
                return new ApiResponseDto(HttpStatusCode.NotFound);

            var blobKey = $"{map.GameType.ToGameType()}_{map.MapName}.jpg";
            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("appdata_storage_connectionstring"));
            var containerClient = blobServiceClient.GetBlobContainerClient("map-images");

            var blobClient = containerClient.GetBlobClient(blobKey);
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
            }

            await blobClient.UploadAsync(filePath);

            map.MapImageUri = blobClient.Uri.ToString();

            await context.SaveChangesAsync();

            return new ApiResponseDto(HttpStatusCode.OK);
        }

        private IQueryable<Map> ApplyFilter(IQueryable<Map> query, GameType? gameType, string[]? mapNames, MapsFilter? filter, string? filterString)
        {
            if (gameType.HasValue)
                query = query.Where(m => m.GameType == gameType.Value.ToGameTypeInt()).AsQueryable();

            if (mapNames != null && mapNames.Length > 0)
                query = query.Where(m => mapNames.Contains(m.MapName)).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterString))
                query = query.Where(m => m.MapName.Contains(filterString)).AsQueryable();

            switch (filter)
            {
                case MapsFilter.EmptyMapImage:
                    query = query.Where(m => m.MapImageUri == null).AsQueryable();
                    break;
            }

            return query;
        }

        private IQueryable<Map> ApplyOrderAndLimits(IQueryable<Map> query, int skipEntries, int takeEntries, MapsOrder? order)
        {
            switch (order)
            {
                case MapsOrder.MapNameAsc:
                    query = query.OrderBy(m => m.MapName).AsQueryable();
                    break;
                case MapsOrder.MapNameDesc:
                    query = query.OrderByDescending(m => m.MapName).AsQueryable();
                    break;
                case MapsOrder.GameTypeAsc:
                    query = query.OrderBy(m => m.GameType).AsQueryable();
                    break;
                case MapsOrder.GameTypeDesc:
                    query = query.OrderByDescending(m => m.GameType).AsQueryable();
                    break;
                case MapsOrder.PopularityAsc:
                    query = query.OrderByDescending(m => m.TotalLikes).AsQueryable();
                    break;
                case MapsOrder.PopularityDesc:
                    query = query.OrderByDescending(m => m.TotalDislikes).AsQueryable();
                    break;
            }

            query = query.Skip(skipEntries).AsQueryable();
            query = query.Take(takeEntries).AsQueryable();

            return query;
        }
    }
}
