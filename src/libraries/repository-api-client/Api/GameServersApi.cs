using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class GameServersApi : BaseApi, IGameServersApi
    {
        public GameServersApi(ILogger<GameServersApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider, IMemoryCache memoryCache) : base(logger, options, repositoryApiTokenProvider)
        {

        }

        public async Task<ApiResponseDto<GameServerDto>> GetGameServer(Guid gameServerId)
        {
            var request = await CreateRequest($"game-servers/{gameServerId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<GameServerDto>();
        }

        public async Task<ApiResponseDto<GameServersCollectionDto>> GetGameServers(GameType[]? gameTypes, Guid[]? serverIds, GameServerFilter? filter, int skipEntries, int takeEntries, GameServerOrder? order)
        {
            var request = await CreateRequest("game-servers", Method.Get);

            if (gameTypes != null)
                request.AddQueryParameter("gameTypes", string.Join(",", gameTypes));

            if (serverIds != null)
                request.AddQueryParameter("serverIds", string.Join(",", serverIds));

            if (filter.HasValue)
                request.AddQueryParameter("filter", filter.ToString());

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (order.HasValue)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<GameServersCollectionDto>();
        }

        public async Task<ApiResponseDto> CreateGameServer(CreateGameServerDto createGameServerDto)
        {
            var request = await CreateRequest("game-servers", Method.Post);
            request.AddJsonBody(new List<CreateGameServerDto> { createGameServerDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CreateGameServers(List<CreateGameServerDto> createGameServerDtos)
        {
            var request = await CreateRequest("game-servers", Method.Post);
            request.AddJsonBody(createGameServerDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> UpdateGameServer(EditGameServerDto editGameServerDto)
        {
            var request = await CreateRequest($"game-servers/{editGameServerDto.Id}", Method.Patch);
            request.AddJsonBody(editGameServerDto);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> DeleteGameServer(Guid gameServerId)
        {
            var request = await CreateRequest($"game-servers/{gameServerId}", Method.Delete);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}