using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.RepositoryApiClient.ReportsApi
{
    public class ReportsApiClient : BaseApiClient, IReportsApiClient
    {
        public ReportsApiClient(ILogger<ReportsApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }
    }
}
