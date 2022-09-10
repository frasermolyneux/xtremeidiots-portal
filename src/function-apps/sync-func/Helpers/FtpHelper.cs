using FluentFTP;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;

namespace XtremeIdiots.Portal.SyncFunc.Helpers
{
    public class FtpHelper : IFtpHelper
    {
        private readonly TelemetryClient telemetryClient;
        private readonly IConfiguration configuration;

        public FtpHelper(
            TelemetryClient telemetryClient,
            IConfiguration configuration)
        {
            this.telemetryClient = telemetryClient;
            this.configuration = configuration;
        }

        public async Task<long?> GetFileSize(string hostname, int port, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetFileSize");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? ftpClient = null;

            try
            {
                ftpClient = new AsyncFtpClient(hostname, username, password, port);
                ftpClient.ValidateCertificate += (control, e) =>
                {
                    if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                    { // Account for self-signed FTP certificate for self-hosted servers
                        e.Accept = true;
                    }
                };

                await ftpClient.AutoConnect();

                if (await ftpClient.FileExists(filePath))
                    return await ftpClient.GetFileSize(filePath);
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
                ftpClient?.Dispose();
            }
        }

        public async Task<DateTime?> GetLastModified(string hostname, int port, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetLastModified");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? ftpClient = null;

            try
            {
                ftpClient = new AsyncFtpClient(hostname, username, password, port);
                ftpClient.ValidateCertificate += (control, e) =>
                {
                    if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                    { // Account for self-signed FTP certificate for self-hosted servers
                        e.Accept = true;
                    }
                };

                await ftpClient.AutoConnect();

                if (await ftpClient.FileExists(filePath))
                    return await ftpClient.GetModifiedTime(filePath);
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
                ftpClient?.Dispose();
            }
        }

        public async Task<string> GetRemoteFileData(string hostname, int port, string filePath, string username, string password)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetRemoteFileData");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? ftpClient = null;

            try
            {
                ftpClient = new AsyncFtpClient(hostname, username, password, port);
                ftpClient.ValidateCertificate += (control, e) =>
                {
                    if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                    { // Account for self-signed FTP certificate for self-hosted servers
                        e.Accept = true;
                    }
                };

                await ftpClient.AutoConnect();

                using (var stream = new MemoryStream())
                {
                    await ftpClient.DownloadStream(stream, filePath);

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
                ftpClient?.Dispose();
            }
        }

        public async Task UpdateRemoteFileFromStream(string hostname, int port, string filePath, string username, string password, Stream data)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>("UpdateRemoteFileFromStream");
            operation.Telemetry.Type = "FTP";
            operation.Telemetry.Target = $"{hostname}:{port}";
            operation.Telemetry.Data = filePath;

            AsyncFtpClient? ftpClient = null;

            try
            {
                ftpClient = new AsyncFtpClient(hostname, username, password, port);
                ftpClient.ValidateCertificate += (control, e) =>
                {
                    if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                    { // Account for self-signed FTP certificate for self-hosted servers
                        e.Accept = true;
                    }
                };

                await ftpClient.AutoConnect();

                data.Seek(0, SeekOrigin.Begin);
                await ftpClient.UploadStream(data, filePath);
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
                ftpClient?.Dispose();
            }
        }
    }
}