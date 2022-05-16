using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XtremeIdiots.Portal.FuncHelpers.Providers;

namespace XtremeIdiots.Portal.ServersApiClient.RconApi
{
    public class RconApiClient : BaseApiClient, IRconApiClient
    {
        public RconApiClient(ILogger<RconApiClient> logger, IOptions<ServersApiClientOptions> options, IServersApiTokenProvider serversApiTokenProvider) : base(logger, options, serversApiTokenProvider)
        {
        }
    }
}
