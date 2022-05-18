using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersStatsApi
{
    public class GameServersStatsApiClient : BaseApiClient, IGameServersStatsApiClient
    {
        public GameServersStatsApiClient(ILogger<GameServersStatsApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }
    }
}
