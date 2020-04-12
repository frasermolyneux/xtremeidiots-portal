using System;

namespace XI.Portal.Servers.Helpers
{
    public interface IFtpHelper
    {
        long GetFileSize(string hostname, string filePath, string username, string password);
        DateTime GetLastModified(string hostname, string filePath, string username, string password);
        string GetRemoteFileData(string hostname, string filePath, string username, string password);
        void UpdateRemoteFile(string hostname, string filePath, string username, string password, string dataPath);
    }
}