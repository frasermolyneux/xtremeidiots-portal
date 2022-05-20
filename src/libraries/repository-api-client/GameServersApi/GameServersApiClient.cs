using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi
{
    public class GameServersApiClient : BaseApiClient, IGameServersApiClient
    {
        private readonly IOptions<RepositoryApiClientOptions> options;
        private readonly IMemoryCache memoryCache;

        public GameServersApiClient(ILogger<GameServersApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider, IMemoryCache memoryCache) : base(logger, options, repositoryApiTokenProvider)
        {
            this.options = options;
            this.memoryCache = memoryCache;
        }

        public async Task<List<GameServerDto>?> GetGameServers(GameType[] gameTypes, Guid[] serverIds, GameServerFilter? filterOption, int skipEntries, int takeEntries, GameServerOrder? order)
        {
            var request = await CreateRequest("repository/game-servers", Method.Get);

            if (gameTypes != null)
                request.AddQueryParameter("gameTypes", string.Join(",", gameTypes));

            if (serverIds != null)
                request.AddQueryParameter("serverIds", string.Join(",", serverIds));

            if (filterOption != null)
                request.AddQueryParameter("filterOption", filterOption.ToString());

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (order != null)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<GameServerDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<GameServerDto?> GetGameServer(Guid serverId)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{serverId}-{nameof(GetGameServer)}", out GameServerDto gameServerDto))
                    return gameServerDto;

            var request = await CreateRequest($"repository/game-servers/{serverId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var gameServerDto = JsonConvert.DeserializeObject<GameServerDto>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && gameServerDto != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{serverId}-{nameof(GetGameServer)}", gameServerDto, cacheEntryOptions);
                }

                return gameServerDto;
            }

            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteGameServer(Guid id)
        {
            var request = await CreateRequest($"repository/game-servers/{id}", Method.Delete);
            await ExecuteAsync(request);
        }

        public async Task CreateGameServer(GameServerDto gameServer)
        {
            var request = await CreateRequest("repository/game-servers", Method.Post);
            request.AddJsonBody(new List<GameServerDto> { gameServer });

            await ExecuteAsync(request);
        }

        public async Task UpdateGameServer(GameServerDto gameServer)
        {
            var request = await CreateRequest($"repository/game-servers/{gameServer.Id}", Method.Patch);
            request.AddJsonBody(gameServer);

            await ExecuteAsync(request);
        }

        public async Task<BanFileMonitorDto?> CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor)
        {
            var request = await CreateRequest($"repository/game-servers/{serverId}/ban-file-monitors", Method.Post);
            request.AddJsonBody(banFileMonitor);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<BanFileMonitorDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}