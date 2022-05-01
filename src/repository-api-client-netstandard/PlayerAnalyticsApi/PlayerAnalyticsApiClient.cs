using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayerAnalyticsApi
{
    public class PlayerAnalyticsApiClient : BaseApiClient, IPlayerAnalyticsApiClient
    {
        public PlayerAnalyticsApiClient(ILogger<PlayerAnalyticsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<List<PlayerAnalyticEntryDto>> GetCumulativeDailyPlayers(string accessToken, DateTime cutoff)
        {
            var request = CreateRequest($"repository/player-analytics/cumulative-daily-players", Method.GET, accessToken);
            request.AddQueryParameter("cutoff", cutoff.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>> GetNewDailyPlayersPerGame(string accessToken, DateTime cutoff)
        {
            var request = CreateRequest($"repository/player-analytics/new-daily-players-per-game", Method.GET, accessToken);
            request.AddQueryParameter("cutoff", cutoff.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticPerGameEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<PlayerAnalyticPerGameEntryDto>> GetPlayersDropOffPerGameJson(string accessToken, DateTime cutoff)
        {
            var request = CreateRequest($"repository/player-analytics/players-drop-off-per-game", Method.GET, accessToken);
            request.AddQueryParameter("cutoff", cutoff.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<PlayerAnalyticPerGameEntryDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}
