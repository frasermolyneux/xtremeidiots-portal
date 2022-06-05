
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

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

        public async Task<ApiResponseDto<MapDto>> GetMap(Guid mapId)
        {
            var request = await CreateRequest($"maps/{mapId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<MapDto>();
        }

        public async Task<ApiResponseDto<MapDto>> GetMap(GameType gameType, string mapName)
        {
            var request = await CreateRequest($"maps/{gameType}/{mapName}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<MapDto>();
        }

        public async Task<ApiResponseDto<MapsCollectionDto>> GetMaps(GameType? gameType, string[]? mapNames, MapsFilter? filter, string? filterString, int skipEntries, int takeEntries, MapsOrder? order)
        {
            var request = await CreateRequest("maps", Method.Get);

            if (gameType.HasValue)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (mapNames != null && mapNames.Length > 0)
                request.AddQueryParameter("mapNames", string.Join(",", mapNames));

            if (filter.HasValue)
                request.AddQueryParameter("filter", filter.ToString());

            if (!string.IsNullOrEmpty(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("skipEntries", skipEntries.ToString());
            request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (order.HasValue)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<MapsCollectionDto>();
        }

        public async Task<ApiResponseDto> CreateMap(CreateMapDto createMapDto)
        {
            var request = await CreateRequest("maps", Method.Post);
            request.AddJsonBody(new List<CreateMapDto> { createMapDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CreateMaps(List<CreateMapDto> createMapDtos)
        {
            var request = await CreateRequest("maps", Method.Post);
            request.AddJsonBody(createMapDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateMap(EditMapDto editMapDto)
        {
            var request = await CreateRequest("maps", Method.Put);
            request.AddJsonBody(new List<EditMapDto> { editMapDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateMaps(List<EditMapDto> editMapDtos)
        {
            var request = await CreateRequest("maps", Method.Put);
            request.AddJsonBody(editMapDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> DeleteMap(Guid mapId)
        {
            var request = await CreateRequest($"maps/{mapId}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> RebuildMapPopularity()
        {
            var request = await CreateRequest($"maps/popularity", Method.Post);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpsertMapVote(Guid mapId, Guid playerId, bool like)
        {
            var request = await CreateRequest($"maps/{mapId}/popularity/{playerId}", Method.Post);
            request.AddQueryParameter("like", like.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateMapImage(Guid mapId, string filePath)
        {
            var request = await CreateRequest($"maps/{mapId}/image", Method.Post);
            request.AddFile("map.jpg", filePath);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
