
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

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

        public async Task<ApiResponseDto<PlayerDto>> GetPlayer(Guid playerId)
        {
            var request = await CreateRequest($"players/{playerId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<PlayerDto>();
        }

        public async Task<ApiResponseDto<PlayerDto>> GetPlayerByGameType(GameType gameType, string guid)
        {
            var request = await CreateRequest($"players/by-game-type/{gameType}/{guid}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<PlayerDto>();
        }

        public async Task<ApiResponseDto> CreatePlayer(CreatePlayerDto createPlayerDto)
        {
            var request = await CreateRequest("players", Method.Post);
            request.AddJsonBody(new List<CreatePlayerDto> { createPlayerDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CreatePlayers(List<CreatePlayerDto> createPlayerDtos)
        {
            var request = await CreateRequest("players", Method.Post);
            request.AddJsonBody(createPlayerDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
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