using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using System.Net;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class PlayersApi : BaseApi, IPlayersApi
    {
        private readonly IOptions<RepositoryApiClientOptions> options;
        private readonly IMemoryCache memoryCache;

        public PlayersApi(ILogger<PlayersApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider, IMemoryCache memoryCache) : base(logger, options, repositoryApiTokenProvider)
        {
            this.options = options;
            this.memoryCache = memoryCache;
        }

        public async Task<PlayerDto?> GetPlayer(Guid playerId)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{playerId}-{nameof(GetPlayer)}", out PlayerDto playerDto))
                    return playerDto;

            var request = await CreateRequest($"players/{playerId}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var playerDto = JsonConvert.DeserializeObject<PlayerDto>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && playerDto != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{playerId}-{nameof(GetPlayer)}", playerDto, cacheEntryOptions);
                }

                return playerDto;
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<AliasDto>?> GetPlayerAliases(Guid playerId)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{playerId}-{nameof(GetPlayerAliases)}", out List<AliasDto> playerAliasDtos))
                    return playerAliasDtos;

            var request = await CreateRequest($"players/{playerId}/aliases", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var playerAliasDtos = JsonConvert.DeserializeObject<List<AliasDto>>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && playerAliasDtos != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{playerId}-{nameof(GetPlayerAliases)}", playerAliasDtos, cacheEntryOptions);
                }

                return playerAliasDtos;
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<IpAddressDto>?> GetPlayerIpAddresses(Guid playerId)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{playerId}-{nameof(GetPlayerIpAddresses)}", out List<IpAddressDto> playerIpAddressDtos))
                    return playerIpAddressDtos;

            var request = await CreateRequest($"players/{playerId}/ip-addresses", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var playerIpAddressDtos = JsonConvert.DeserializeObject<List<IpAddressDto>>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && playerIpAddressDtos != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{playerId}-{nameof(GetPlayerIpAddresses)}", playerIpAddressDtos, cacheEntryOptions);
                }

                return playerIpAddressDtos;
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<RelatedPlayerDto>?> GetRelatedPlayers(Guid playerId, string ipAddress)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{playerId}-{ipAddress}-{nameof(GetRelatedPlayers)}", out List<RelatedPlayerDto> relatedPlayerDtos))
                    return relatedPlayerDtos;

            var request = await CreateRequest($"players/{playerId}/related-players", Method.Get);
            request.AddQueryParameter("IpAddress", ipAddress);

            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var relatedPlayerDtos = JsonConvert.DeserializeObject<List<RelatedPlayerDto>>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && relatedPlayerDtos != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{playerId}-{ipAddress}-{nameof(GetRelatedPlayers)}", relatedPlayerDtos, cacheEntryOptions);
                }

                return relatedPlayerDtos;
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<PlayerDto?> GetPlayerByGameType(GameType gameType, string guid)
        {
            if (options.Value.UseMemoryCacheOnGet)
                if (memoryCache.TryGetValue($"{gameType}-{guid}-{nameof(GetPlayerByGameType)}", out PlayerDto playerDto))
                    return playerDto;

            var request = await CreateRequest($"players/by-game-type/{gameType}/{guid}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
            {
                var playerDto = JsonConvert.DeserializeObject<PlayerDto>(response.Content);

                if (options.Value.UseMemoryCacheOnGet && playerDto != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(options.Value.MemoryCacheOnGetExpiration));
                    memoryCache.Set($"{gameType}-{guid}-{nameof(GetPlayerByGameType)}", playerDto, cacheEntryOptions);
                }

                return playerDto;
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task CreatePlayer(CreatePlayerDto createPlayerDto)
        {
            var request = await CreateRequest("players", Method.Post);
            request.AddJsonBody(new List<CreatePlayerDto> { createPlayerDto });

            await ExecuteAsync(request);
        }

        public async Task UpdatePlayer(PlayerDto player)
        {
            var request = await CreateRequest($"players/{player.Id}", Method.Patch);
            request.AddJsonBody(player);

            await ExecuteAsync(request);
        }

        public async Task<PlayersSearchResponseDto?> SearchPlayers(string gameType, string filter, string filterString, int takeEntries, int skipEntries, string? order)
        {
            var request = await CreateRequest("players/search", Method.Get);

            if (!string.IsNullOrWhiteSpace(gameType))
                request.AddQueryParameter("gameType", gameType);

            if (!string.IsNullOrWhiteSpace(filter))
                request.AddQueryParameter("filter", filter);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<PlayersSearchResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<AdminActionDto>?> GetAdminActionsForPlayer(Guid playerId)
        {
            var request = await CreateRequest($"players/{playerId}/admin-actions", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<AdminActionDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<AdminActionDto?> CreateAdminActionForPlayer(AdminActionDto adminAction)
        {
            var request = await CreateRequest($"players/{adminAction.PlayerId}/admin-actions", Method.Post);
            request.AddJsonBody(adminAction);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<AdminActionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<AdminActionDto?> UpdateAdminActionForPlayer(AdminActionDto adminAction)
        {
            var request = await CreateRequest($"players/{adminAction.PlayerId}/admin-actions/{adminAction.AdminActionId}", Method.Patch);
            request.AddJsonBody(adminAction);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<AdminActionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}