
using FluentFTP;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net;

using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApi.Abstractions;
using XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.ServersWebApi.Extensions;

namespace XtremeIdiots.Portal.ServersWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class MapsController : Controller, IMapsApi
    {
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly IConfiguration configuration;

        public MapsController(
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            IConfiguration configuration)
        {
            this.repositoryApiClient = repositoryApiClient;
            this.telemetryClient = telemetryClient;
            this.configuration = configuration;
        }

        [HttpGet]
        [Route("maps/{gameServerId}")]
        public async Task<IActionResult> GetServerMaps(Guid gameServerId)
        {
            var response = await ((IMapsApi)this).GetServerMaps(gameServerId);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto<ServerMapsCollectionDto>> IMapsApi.GetServerMaps(Guid gameServerId)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(gameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return new ApiResponseDto<ServerMapsCollectionDto>(HttpStatusCode.NotFound);

            var operation = telemetryClient.StartOperation<DependencyTelemetry>("GetFileList");
            operation.Telemetry.Type = $"FTP";
            operation.Telemetry.Target = $"{gameServerApiResponse.Result.FtpHostname}:{gameServerApiResponse.Result.FtpPort}";

            AsyncFtpClient? ftpClient = null;

            try
            {
                ftpClient = new AsyncFtpClient(gameServerApiResponse.Result.FtpHostname, gameServerApiResponse.Result.FtpUsername, gameServerApiResponse.Result.FtpPassword, gameServerApiResponse.Result.FtpPort.Value);
                ftpClient.ValidateCertificate += (control, e) =>
                {
                    if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                    { // Account for self-signed FTP certificate for self-hosted servers
                        e.Accept = true;
                    }
                };

                await ftpClient.AutoConnect();
                await ftpClient.SetWorkingDirectory("usermaps");

                var files = await ftpClient.GetListing();

                var result = new ServerMapsCollectionDto
                {
                    TotalRecords = files.Count(),
                    FilteredRecords = files.Count(),
                    Entries = files.Select(f => new ServerMapDto(f.Name, f.FullName, f.Modified)).ToList()
                };

                return new ApiResponseDto<ServerMapsCollectionDto>(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = ex.Message;
                telemetryClient.TrackException(ex);

                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
                ftpClient?.Dispose();
            }
        }

        [HttpPost]
        [Route("maps/{gameServerId}/{mapName}")]
        public async Task<IActionResult> PushServerMap(Guid gameServerId, string mapName)
        {
            var response = await ((IMapsApi)this).PushServerMap(gameServerId, mapName);

            return response.ToHttpResult();
        }

        async Task<ApiResponseDto> IMapsApi.PushServerMap(Guid gameServerId, string mapName)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(gameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return new ApiResponseDto<ServerMapsCollectionDto>(HttpStatusCode.NotFound);

            var mapApiResponse = await repositoryApiClient.Maps.GetMap(gameServerApiResponse.Result.GameType, mapName);

            if (mapApiResponse.IsNotFound || mapApiResponse.Result == null)
                return new ApiResponseDto<ServerMapsCollectionDto>(HttpStatusCode.NotFound, "Map could not be found in the database");

            AsyncFtpClient? ftpClient = null;

            try
            {
                ftpClient = new AsyncFtpClient(gameServerApiResponse.Result.FtpHostname, gameServerApiResponse.Result.FtpUsername, gameServerApiResponse.Result.FtpPassword, gameServerApiResponse.Result.FtpPort.Value);
                ftpClient.ValidateCertificate += (control, e) =>
                {
                    if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                    { // Account for self-signed FTP certificate for self-hosted servers
                        e.Accept = true;
                    }
                };

                await ftpClient.AutoConnect();

                var mapDirectoryPath = $"usermaps/{mapName}";

                if (!await ftpClient.DirectoryExists(mapDirectoryPath))
                {
                    await ftpClient.CreateDirectory(mapDirectoryPath);
                }

                foreach (var file in mapApiResponse.Result.MapFiles)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var filePath = Path.Join(Path.GetTempPath(), file.FileName);
                        using (var stream = System.IO.File.Create(filePath))
                            await (await httpClient.GetStreamAsync(file.Url)).CopyToAsync(stream);

                        await ftpClient.UploadFile(filePath, $"{mapDirectoryPath}/{file.FileName}");
                    }
                }

                return new ApiResponseDto(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                ftpClient?.Dispose();
            }
        }
    }
}
