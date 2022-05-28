using FluentFTP;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace XtremeIdiots.Portal.SyncFunc.Helpers
{
    public class FtpHelper : IFtpHelper
    {
        private readonly TelemetryClient telemetryClient;

        public FtpHelper(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }

        public async Task<long?> GetFileSize(string hostname, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetFileSize");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = hostname;
            operation.Telemetry.Data = filePath;

            FtpClient? client = null;

            try
            {
                client = new FtpClient(hostname, username, password);
                await client.ConnectAsync();

                if (await client.FileExistsAsync(filePath))
                    return await client.GetFileSizeAsync(filePath);
                else
                    return null;
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = "Failed";
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
                client?.Dispose();
            }
        }

        public async Task<DateTime?> GetLastModified(string hostname, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetLastModified");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = hostname;
            operation.Telemetry.Data = filePath;

            FtpClient? client = null;

            try
            {
                client = new FtpClient(hostname, username, password);
                await client.ConnectAsync();

                if (await client.FileExistsAsync(filePath))
                    return await client.GetModifiedTimeAsync(filePath);
                else
                    return null;
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = "Failed";
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
                client?.Dispose();
            }
        }

        public async Task<string> GetRemoteFileData(string hostname, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetRemoteFileData");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = hostname;
            operation.Telemetry.Data = filePath;

            FtpClient? client = null;

            try
            {
                client = new FtpClient(hostname, username, password);
                await client.ConnectAsync();

                using (var stream = new MemoryStream())
                {
                    await client.DownloadAsync(stream, filePath);

                    using (var streamReader = new StreamReader(stream))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = "Failed";
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
                client?.Dispose();
            }
        }

        public async Task UpdateRemoteFileFromStream(string hostname, string filePath, string username, string password, Stream data)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("UpdateRemoteFileFromStream");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = hostname;
            operation.Telemetry.Data = filePath;

            FtpClient? client = null;

            try
            {
                client = new FtpClient(hostname, username, password);
                await client.ConnectAsync();
                data.Seek(0, SeekOrigin.Begin);
                await client.UploadAsync(data, filePath);
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = "Failed";
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
                client?.Dispose();
            }
        }
    }
}