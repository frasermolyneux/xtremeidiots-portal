using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;

namespace XtremeIdiots.Portal.RepositoryApiClient.RecentPlayersApi
{
    public class RecentPlayersApiClient : BaseApiClient, IRecentPlayersApiClient
    {
        public RecentPlayersApiClient(ILogger<RecentPlayersApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<RecentPlayersCollectionDto?> GetRecentPlayers(GameType? gameType, Guid? serverId, DateTime? cutoff, RecentPlayersFilter? filterType, int skipEntries, int takeEntries, RecentPlayersOrder? order)
        {
            var request = await CreateRequest("repository/recent-players", Method.Get);

            if (gameType.HasValue)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (serverId.HasValue)
                request.AddQueryParameter("serverId", serverId.ToString());

            if (cutoff.HasValue)
                request.AddQueryParameter("cutoff", cutoff.Value.ToString("MM/dd/yyyy HH:mm:ss"));

            if (filterType.HasValue)
                request.AddQueryParameter("filterType", filterType.ToString());

            request.AddQueryParameter("skipEntries", skipEntries.ToString());
            request.AddQueryParameter("takeEntries", takeEntries.ToString());

            if (order != null)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<RecentPlayersCollectionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task CreateRecentPlayers(List<CreateRecentPlayerDto> createRecentPlayerDtos)
        {
            var request = await CreateRequest("repository/recent-players", Method.Post);
            request.AddJsonBody(createRecentPlayerDtos);

            var response = await ExecuteAsync(request);
        }
    }
}
