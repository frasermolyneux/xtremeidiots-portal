using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayerAnalyticsApi
{
    public class PlayerAnalyticsApiClient : BaseApiClient, IPlayerAnalyticsApiClient
    {
        public PlayerAnalyticsApiClient(ILogger<PlayerAnalyticsApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<List<PlayerAnalyticEntryDto>?> GetCumulativeDailyPlayers(DateTime cutoff)
        {
            var request = await CreateRequest($"repository/player-analytics/cumulative-daily-players", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>?> GetNewDailyPlayersPerGame(DateTime cutoff)
        {
            var request = await CreateRequest($"repository/player-analytics/new-daily-players-per-game", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticPerGameEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>?> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var request = await CreateRequest($"repository/player-analytics/players-drop-off-per-game", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticPerGameEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
