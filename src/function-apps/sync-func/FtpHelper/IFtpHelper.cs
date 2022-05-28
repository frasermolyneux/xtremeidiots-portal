using System;
using System.IO;
using System.Threading.Tasks;

namespace XtremeIdiots.Portal.SyncFunc.FtpHelper
{
    public interface IFtpHelper
    {
        long GetFileSize(string hostname, string filePath, string username, string password);
        DateTime GetLastModified(string hostname, string filePath, string username, string password);
        string GetRemoteFileData(string hostname, string filePath, string username, string password);
        void UpdateRemoteFile(string hostname, string filePath, string username, string password, string dataPath);
        Task UpdateRemoteFileFromStream(string hostname, string filePath, string username, string password, Stream data);
    }
}