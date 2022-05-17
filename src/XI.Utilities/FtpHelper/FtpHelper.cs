using System.Net;
using System.Text;

namespace XI.Utilities.FtpHelper
{
    public class FtpHelper : IFtpHelper
    {
        public long GetFileSize(string hostname, string filePath, string username, string password)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create($"ftp://{hostname}/{filePath}");
                request.Method = WebRequestMethods.Ftp.GetFileSize;
                request.Credentials = new NetworkCredential(username, password);

                return ((FtpWebResponse)request.GetResponse()).ContentLength;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("(550) File unavailable"))
                    return 0;

                throw;
            }

        }

        public DateTime GetLastModified(string hostname, string filePath, string username, string password)
        {
            var request = (FtpWebRequest)WebRequest.Create($"ftp://{hostname}/{filePath}");
            request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            request.Credentials = new NetworkCredential(username, password);

            return ((FtpWebResponse)request.GetResponse()).LastModified;
        }

        public string GetRemoteFileData(string hostname, string filePath, string username, string password)
        {
            var request = CreateWebRequest(hostname, filePath, username, password);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream ?? throw new InvalidOperationException()))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }

        public void UpdateRemoteFile(string hostname, string filePath, string username, string password, string dataPath)
        {
            var request = CreateWebRequest(hostname, filePath, username, password);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            using (var streamReader = new StreamReader(dataPath))
            {
                var fileContents = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());

                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }
            }
        }

        public async Task UpdateRemoteFileFromStream(string hostname, string filePath, string username, string password, Stream data)
        {
            var request = CreateWebRequest(hostname, filePath, username, password);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            using (var streamReader = new StreamReader(data))
            {
                var fileContents = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());

                using (var requestStream = request.GetRequestStream())
                {
                    await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
                }
            }
        }

        private static WebRequest CreateWebRequest(string hostname, string filePath, string username, string password)
        {
            var requestPath = $"ftp://{hostname}{filePath}";

            var request = WebRequest.Create(new Uri(requestPath));
            request.Credentials = new NetworkCredential(username, password);
            return request;
        }
    }
}