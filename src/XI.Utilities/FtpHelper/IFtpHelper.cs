using System;

namespace XI.Utilities.FtpHelper
{
    public interface IFtpHelper
    {
        long GetFileSize(string hostname, string filePath, string username, string password);
        DateTime GetLastModified(string hostname, string filePath, string username, string password);
        string GetRemoteFileData(string hostname, string filePath, string username, string password);
        void UpdateRemoteFile(string hostname, string filePath, string username, string password, string dataPath);
    }
}