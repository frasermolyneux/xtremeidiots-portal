using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.RepositoryApiClient.RecentPlayersApi
{
    public class RecentPlayersApiClient : BaseApiClient, IRecentPlayersApiClient
    {
        public RecentPlayersApiClient(ILogger<RecentPlayersApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }
    }
}
