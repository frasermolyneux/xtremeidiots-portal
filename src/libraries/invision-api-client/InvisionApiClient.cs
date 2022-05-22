using XtremeIdiots.Portal.InvisionApiClient.CoreApi;
using XtremeIdiots.Portal.InvisionApiClient.DownloadsApi;
using XtremeIdiots.Portal.InvisionApiClient.ForumsApi;

namespace XtremeIdiots.Portal.InvisionApiClient
{
    public class InvisionApiClient : IInvisionApiClient
    {
        public InvisionApiClient(
            ICoreApiClient coreApiClient,
            IDownloadsApiClient downloadsApiClient,
            IForumsApiClient forumsApiClient)
        {
            Core = coreApiClient;
            Downloads = downloadsApiClient;
            Forums = forumsApiClient;
        }

        public ICoreApiClient Core { get; }
        public IDownloadsApiClient Downloads { get; }
        public IForumsApiClient Forums { get; }
    }
}