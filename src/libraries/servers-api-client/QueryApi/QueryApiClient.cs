using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.ServersApiClient.QueryApi
{
    public class QueryApiClient : BaseApiClient, IQueryApiClient
    {
        public QueryApiClient(ILogger<QueryApiClient> logger, IOptions<ServersApiClientOptions> options, IServersApiTokenProvider serversApiTokenProvider) : base(logger, options, serversApiTokenProvider)
        {
        }
    }
}
