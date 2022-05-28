namespace XtremeIdiots.Portal.SyncFunc.Helpers
{
    public interface IFtpHelper
    {
        Task<long?> GetFileSize(string hostname, int port, string filePath, string username, string password);
        Task<DateTime?> GetLastModified(string hostname, int port, string filePath, string username, string password);
        Task<string> GetRemoteFileData(string hostname, int port, string filePath, string username, string password);
        Task UpdateRemoteFileFromStream(string hostname, int port, string filePath, string username, string password, Stream data);
    }
}