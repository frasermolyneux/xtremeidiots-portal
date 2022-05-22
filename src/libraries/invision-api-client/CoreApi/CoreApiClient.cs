using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.InvisionApiClient.CoreApi
{
    public class CoreApiClient : BaseApiClient, ICoreApiClient
    {
        public CoreApiClient(ILogger<CoreApiClient> logger, IOptions<InvisionApiClientOptions> options) : base(logger, options)
        {
        }
    }
}
