using XtremeIdiots.Portal.InvisionApiClient.Interfaces;

namespace XtremeIdiots.Portal.InvisionApiClient
{
    public class InvisionApiClient : IInvisionApiClient
    {
        public InvisionApiClient(
            ICoreApi coreApiClient,
            IDownloadsApi downloadsApiClient,
            IForumsApi forumsApiClient)
        {
            Core = coreApiClient;
            Downloads = downloadsApiClient;
            Forums = forumsApiClient;
        }

        public ICoreApi Core { get; }
        public IDownloadsApi Downloads { get; }
        public IForumsApi Forums { get; }
    }
}