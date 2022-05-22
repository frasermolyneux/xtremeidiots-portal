using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.DownloadsApi
{
    public interface IDownloadsApiClient
    {
        Task<DownloadFile?> GetDownloadFile(int fileId);
    }
}