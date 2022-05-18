using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.RepositoryApiClient.LivePlayersApi
{
    public class LivePlayersApiClient : BaseApiClient, ILivePlayersApiClient
    {
        public LivePlayersApiClient(ILogger<LivePlayersApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }
    }
}
