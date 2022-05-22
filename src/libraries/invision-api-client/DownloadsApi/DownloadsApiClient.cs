using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.InvisionApiClient.DownloadsApi
{
    public class DownloadsApiClient : BaseApiClient, IDownloadsApiClient
    {
        public DownloadsApiClient(ILogger<DownloadsApiClient> logger, IOptions<InvisionApiClientOptions> options) : base(logger, options)
        {
        }
    }
}
