using XtremeIdiots.Portal.InvisionApiClient.Models;

namespace XtremeIdiots.Portal.InvisionApiClient.Interfaces
{
    public interface IDownloadsApi
    {
        Task<DownloadFile?> GetDownloadFile(int fileId);
    }
}