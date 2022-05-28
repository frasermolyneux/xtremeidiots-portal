namespace XtremeIdiots.Portal.SyncFunc.FtpHelper
{
    public interface IFtpHelper
    {
        Task<long?> GetFileSize(string hostname, string filePath, string username, string password);
        Task<DateTime?> GetLastModified(string hostname, string filePath, string username, string password);
        Task<string> GetRemoteFileData(string hostname, string filePath, string username, string password);
        Task UpdateRemoteFileFromStream(string hostname, string filePath, string username, string password, Stream data);
    }
}