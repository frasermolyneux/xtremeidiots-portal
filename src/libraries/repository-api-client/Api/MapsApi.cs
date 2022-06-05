using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using System.Net;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class MapsApi : BaseApi, IMapsApi
    {
        private readonly IOptions<RepositoryApiClientOptions> options;
        private readonly IMemoryCache memoryCache;

        public MapsApi(ILogger<MapsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider, IMemoryCache memoryCache) : base(logger, options, repositoryApiTokenProvider)
        {
            this.options = options;
            this.memoryCache = memoryCache;
        }

        public async Task<MapDto?> CreateMap(MapDto mapDto)
        {
            var request = await CreateRequest("maps", Method.Post);
            request.AddJsonBody(new List<MapDto> { mapDto });

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<MapDto>>(response.Content);
                return result?.FirstOrDefault() ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entity");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<MapDto>?> CreateMaps(List<MapDto> mapDtos)
        {
            var request = await CreateRequest("maps", Method.Post);
            request.AddJsonBody(mapDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<MapDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteMap(Guid mapId)
        {
            var request = await CreateRequest($"maps/{mapId}", Method.Delete);
            await ExecuteAsync(request);
        }

        public async Task<MapDto?> GetMap(Guid mapId)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{mapId}-{nameof(GetMap)}", out MapDto mapDto))
                    return mapDto;

            var request = await CreateRequest($"maps/{mapId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var mapDto = JsonConvert.DeserializeObject<MapDto>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && mapDto != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{mapId}-{nameof(GetMap)}", mapDto, cacheEntryOptions);
                }

                return mapDto;
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<MapDto?> GetMap(GameType gameType, string mapName)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{gameType}-{mapName}-{nameof(GetMap)}", out MapDto mapDto))
                    return mapDto;

            var request = await CreateRequest($"maps/{gameType}/{mapName}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var mapDto = JsonConvert.DeserializeObject<MapDto>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && mapDto != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{gameType}-{mapName}-{nameof(GetMap)}", mapDto, cacheEntryOptions);
                }

                return mapDto;
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<MapsResponseDto?> GetMaps(GameType? gameType, string[]? mapNames, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order)
        {
            var request = await CreateRequest("maps", Method.Get);

            if (gameType != null)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (mapNames != null && mapNames.Length > 0)
                request.AddQueryParameter("mapNames", string.Join(",", mapNames));

            if (!string.IsNullOrEmpty(filterString))
                request.AddQueryParameter("filterString", filterString);

            if (skipEntries != null)
                request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (takeEntries != null)
                request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (order != null)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<MapsResponseDto>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' is invalid");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<MapDto?> UpdateMap(MapDto mapDto)
        {
            var request = await CreateRequest("maps", Method.Put);
            request.AddJsonBody(new List<MapDto> { mapDto });

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<MapDto>>(response.Content);
                return result?.FirstOrDefault() ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entity");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<MapDto>?> UpdateMaps(List<MapDto> mapDtos)
        {
            var request = await CreateRequest("maps", Method.Put);
            request.AddJsonBody(mapDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<MapDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task RebuildMapPopularity()
        {
            var request = await CreateRequest($"maps/popularity", Method.Post);
            await ExecuteAsync(request);
        }

        public async Task UpsertMapVote(Guid mapId, Guid playerId, bool like, DateTime? overrideCreated = null)
        {
            var request = await CreateRequest($"maps/{mapId}/popularity/{playerId}", Method.Post);
            request.AddQueryParameter("like", like.ToString());

            if (overrideCreated != null)
                request.AddQueryParameter("overrideCreated", overrideCreated.ToString());

            await ExecuteAsync(request);
        }

        public async Task UpdateMapImage(Guid mapId, string filePath)
        {
            var request = await CreateRequest($"maps/{mapId}/image", Method.Post);
            request.AddFile("map.jpg", filePath);

            await ExecuteAsync(request);
        }
    }
}
