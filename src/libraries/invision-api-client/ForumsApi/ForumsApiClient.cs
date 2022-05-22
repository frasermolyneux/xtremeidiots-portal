using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.InvisionApiClient.ForumsApi
{
    public class ForumsApiClient : BaseApiClient, IForumsApiClient
    {
        public ForumsApiClient(ILogger<ForumsApiClient> logger, IOptions<InvisionApiClientOptions> options) : base(logger, options)
        {
        }
    }
}
