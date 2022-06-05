using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class PlayerAnalyticsApi : BaseApi, IPlayerAnalyticsApi
    {
        public PlayerAnalyticsApi(ILogger<PlayerAnalyticsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<List<PlayerAnalyticEntryDto>?> GetCumulativeDailyPlayers(DateTime cutoff)
        {
            var request = await CreateRequest($"player-analytics/cumulative-daily-players", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString("MM/dd/yyyy HH:mm:ss"));

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>?> GetNewDailyPlayersPerGame(DateTime cutoff)
        {
            var request = await CreateRequest($"player-analytics/new-daily-players-per-game", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString("MM/dd/yyyy HH:mm:ss"));

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticPerGameEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>?> GetPlayersDropOffPerGameJson(DateTime cutoff)
        {
            var request = await CreateRequest($"player-analytics/players-drop-off-per-game", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString("MM/dd/yyyy HH:mm:ss"));

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticPerGameEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
