using XtremeIdiots.Portal.InvisionApiClient.Interfaces;

namespace XtremeIdiots.Portal.InvisionApiClient
{
    public interface IInvisionApiClient
    {
        ICoreApi Core { get; }
        IDownloadsApi Downloads { get; }
        IForumsApi Forums { get; }
    }
}