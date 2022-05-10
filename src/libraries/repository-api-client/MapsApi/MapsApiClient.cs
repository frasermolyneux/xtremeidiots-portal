using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.MapsApi
{
    public class MapsApiClient : BaseApiClient, IMapsApiClient
    {
        public MapsApiClient(ILogger<MapsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<MapDto> CreateMap(string accessToken, MapDto mapDto)
        {
            var request = CreateRequest("repository/maps", Method.Post, accessToken);
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

        public async Task<List<MapDto>> CreateMaps(string accessToken, List<MapDto> mapDtos)
        {
            var request = CreateRequest("repository/maps", Method.Post, accessToken);
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

        public async Task DeleteMap(string accessToken, Guid mapId)
        {
            var request = CreateRequest($"repository/maps/{mapId}", Method.Delete, accessToken);
            await ExecuteAsync(request);
        }

        public async Task<MapDto?> GetMap(string accessToken, Guid mapId)
        {
            var request = CreateRequest($"repository/maps/{mapId}", Method.Get, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<MapDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<MapDto?> GetMap(string accessToken, GameType gameType, string mapName)
        {
            var request = CreateRequest($"repository/maps/{gameType}/{mapName}", Method.Get, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<MapDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<MapsResponseDto> GetMaps(string accessToken, GameType? gameType, string[]? mapNames, string? filterString, int? skipEntries, int? takeEntries, MapsOrder? order)
        {
            var request = CreateRequest("repository/maps", Method.Get, accessToken);

            if (gameType == null)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (mapNames != null && mapNames.Count() > 0)
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

        public async Task<MapDto> UpdateMap(string accessToken, MapDto mapDto)
        {
            var request = CreateRequest("repository/maps", Method.Put, accessToken);
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

        public async Task<List<MapDto>> UpdateMaps(string accessToken, List<MapDto> mapDtos)
        {
            var request = CreateRequest("repository/maps", Method.Put, accessToken);
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

        public async Task RebuildMapPopularity(string accessToken)
        {
            var request = CreateRequest($"repository/maps/popularity", Method.Post, accessToken);
            await ExecuteAsync(request);
        }

        public async Task UpsertMapVote(string accessToken, Guid mapId, Guid playerId, bool like, DateTime? overrideCreated = null)
        {
            var request = CreateRequest($"repository/maps/{mapId}/popularity/{playerId}", Method.Post, accessToken);
            request.AddQueryParameter("like", like.ToString());

            if (overrideCreated != null)
                request.AddQueryParameter("overrideCreated", overrideCreated.ToString());

            await ExecuteAsync(request);
        }

        public async Task UpdateMapImage(string accessToken, Guid mapId, byte[] imageData)
        {
            var request = CreateRequest($"repository/maps/{mapId}/image", Method.Post, accessToken);
            request.AddBody(imageData);

            await ExecuteAsync(request);
        }
    }
}
