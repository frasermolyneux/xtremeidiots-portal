using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class LivePlayersApi : BaseApi, ILivePlayersApi
    {
        public LivePlayersApi(ILogger<LivePlayersApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<LivePlayersCollectionDto>> GetLivePlayers(GameType? gameType, Guid? serverId, LivePlayerFilter? filter, int skipEntries, int takeEntries, LivePlayersOrder? order)
        {
            var request = await CreateRequest($"repository/live-players", Method.Get);

            if (gameType.HasValue)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (serverId.HasValue)
                request.AddQueryParameter("serverId", serverId.ToString());

            if (filter.HasValue)
                request.AddQueryParameter("filterType", filter.ToString());

            request.AddQueryParameter("skipEntries", skipEntries.ToString());
            request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (order != null)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<LivePlayersCollectionDto>();
        }

        public async Task<ApiResponseDto> SetLivePlayersForGameServer(Guid serverId, List<CreateLivePlayerDto> createLivePlayerDtos)
        {
            var request = await CreateRequest($"repository/live-players/{serverId}", Method.Post);
            request.AddJsonBody(createLivePlayerDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}
