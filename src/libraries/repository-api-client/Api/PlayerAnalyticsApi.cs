using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class PlayerAnalyticsApi : BaseApi, IPlayerAnalyticsApi
    {
        public PlayerAnalyticsApi(ILogger<PlayerAnalyticsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto<PlayerAnalyticEntryCollectionDto>> GetCumulativeDailyPlayers(DateTime cutoff)
        {
            var request = await CreateRequest($"player-analytics/cumulative-daily-players", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString("MM/dd/yyyy HH:mm:ss"));

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<PlayerAnalyticEntryCollectionDto>();
        }

        public async Task<ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>> GetNewDailyPlayersPerGame(DateTime cutoff)
        {
            var request = await CreateRequest($"player-analytics/new-daily-players-per-game", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString("MM/dd/yyyy HH:mm:ss"));

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<PlayerAnalyticPerGameEntryCollectionDto>();
        }

        public async Task<ApiResponseDto<PlayerAnalyticPerGameEntryCollectionDto>> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var request = await CreateRequest($"player-analytics/players-drop-off-per-game", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString("MM/dd/yyyy HH:mm:ss"));

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<PlayerAnalyticPerGameEntryCollectionDto>();
        }
    }
}
