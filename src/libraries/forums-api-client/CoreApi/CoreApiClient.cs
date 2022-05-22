using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.ForumsApiClient.CoreApi
{
    public class CoreApiClient : BaseApiClient, ICoreApiClient
    {
        public CoreApiClient(ILogger<CoreApiClient> logger, IOptions<ForumsApiClientOptions> options) : base(logger, options)
        {
        }
    }
}
