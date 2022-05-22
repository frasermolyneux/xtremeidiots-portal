using XtremeIdiots.Portal.InvisionApiClient.CoreApi;
using XtremeIdiots.Portal.InvisionApiClient.DownloadsApi;
using XtremeIdiots.Portal.InvisionApiClient.ForumsApi;

namespace XtremeIdiots.Portal.InvisionApiClient
{
    public interface IInvisionApiClient
    {
        ICoreApiClient Core { get; }
        IDownloadsApiClient Downloads { get; }
        IForumsApiClient Forums { get; }
    }
}