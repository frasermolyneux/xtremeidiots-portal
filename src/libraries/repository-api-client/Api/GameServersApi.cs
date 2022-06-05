using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class GameServersApi : BaseApi, IGameServersApi
    {
        private readonly IOptions<RepositoryApiClientOptions> options;
        private readonly IMemoryCache memoryCache;

        public GameServersApi(ILogger<GameServersApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider, IMemoryCache memoryCache) : base(logger, options, repositoryApiTokenProvider)
        {
            this.options = options;
            this.memoryCache = memoryCache;
        }

        public async Task<ApiResponseDto<GameServerDto>> GetGameServer(Guid serverId)
        {
            var request = await CreateRequest($"game-servers/{serverId}", Method.Get);
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

        public async Task DeleteGameServer(Guid id)
        {
            var request = await CreateRequest($"game-servers/{id}", Method.Delete);
            await ExecuteAsync(request);
        }

        public async Task CreateGameServer(CreateGameServerDto createGameServerDto)
        {
            var request = await CreateRequest("game-servers", Method.Post);
            request.AddJsonBody(new List<CreateGameServerDto> { createGameServerDto });

            await ExecuteAsync(request);
        }

        public async Task CreateGameServers(List<CreateGameServerDto> createGameServerDtos)
        {
            var request = await CreateRequest("game-servers", Method.Post);
            request.AddJsonBody(createGameServerDtos);

            await ExecuteAsync(request);
        }

        public async Task UpdateGameServer(GameServerDto gameServer)
        {
            var request = await CreateRequest($"game-servers/{gameServer.Id}", Method.Patch);
            request.AddJsonBody(gameServer);

            await ExecuteAsync(request);
        }

        public async Task<BanFileMonitorDto?> CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor)
        {
            var request = await CreateRequest($"game-servers/{serverId}/ban-file-monitors", Method.Post);
            request.AddJsonBody(banFileMonitor);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<BanFileMonitorDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}