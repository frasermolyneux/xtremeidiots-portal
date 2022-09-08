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

        public async Task<long?> GetFileSize(string hostname, int port, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetFileSize");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? client = null;

            try
            {
                client = new AsyncFtpClient(hostname, username, password, port);
                client.ValidateCertificate += (control, e) => { };

                await client.AutoConnect();

                if (await client.FileExists(filePath))
                    return await client.GetFileSize(filePath);
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

        public async Task<DateTime?> GetLastModified(string hostname, int port, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetLastModified");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? client = null;

            try
            {
                client = new AsyncFtpClient(hostname, username, password, port);
                client.ValidateCertificate += (control, e) => { };

                await client.AutoConnect();

                if (await client.FileExists(filePath))
                    return await client.GetModifiedTime(filePath);
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

        public async Task<string> GetRemoteFileData(string hostname, int port, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetRemoteFileData");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? client = null;

            try
            {
                client = new AsyncFtpClient(hostname, username, password, port);
                client.ValidateCertificate += (control, e) => { };

                await client.AutoConnect();

                using (var stream = new MemoryStream())
                {
                    await client.DownloadStream(stream, filePath);

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

        public async Task UpdateRemoteFileFromStream(string hostname, int port, string filePath, string username, string password, Stream data)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("UpdateRemoteFileFromStream");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? client = null;

            try
            {
                client = new AsyncFtpClient(hostname, username, password, port);
                client.ValidateCertificate += (control, e) => { };

                await client.AutoConnect();

                data.Seek(0, SeekOrigin.Begin);
                await client.UploadStream(data, filePath);
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