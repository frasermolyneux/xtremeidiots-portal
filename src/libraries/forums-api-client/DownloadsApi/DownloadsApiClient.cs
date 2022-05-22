using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XtremeIdiots.Portal.ForumsApiClient.DownloadsApi
{
    public class DownloadsApiClient : BaseApiClient, IDownloadsApiClient
    {
        public DownloadsApiClient(ILogger<DownloadsApiClient> logger, IOptions<ForumsApiClientOptions> options) : base(logger, options)
        {
        }
    }
}
