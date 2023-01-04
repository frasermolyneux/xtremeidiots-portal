using FluentFTP;
using FluentFTP.Logging;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc
{
    public class UpdateBanFileMonitorConfig
    {
        private readonly ILogger<UpdateBanFileMonitorConfig> logger;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IConfiguration configuration;

        public UpdateBanFileMonitorConfig(
            ILogger<UpdateBanFileMonitorConfig> logger,
            IRepositoryApiClient repositoryApiClient,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.configuration = configuration;
        }

        [FunctionName("UpdateBanFileMonitorConfig")]
        public async Task RunUpdateBanFileMonitorConfig([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer)
        {
            GameType[] gameTypes = new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5 };
            var gameServersApiResponse = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, null, 0, 50, null);
            var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(gameTypes, null, null, 0, 50, null);

            if (!gameServersApiResponse.IsSuccess)
            {
                logger.LogCritical("Failed to retrieve game servers from repository");
                return;
            }

            if (!banFileMonitorsApiResponse.IsSuccess)
            {
                logger.LogCritical("Failed to retrieve ban file monitors from repository");
                return;
            }

            foreach (var gameServerDto in gameServersApiResponse.Result.Entries)
            {
                if (string.IsNullOrWhiteSpace(gameServerDto.LiveMod))
                    continue;

                var banFileMonitorDto = banFileMonitorsApiResponse.Result.Entries.SingleOrDefault(bfm => bfm.GameServerId == gameServerDto.GameServerId);

                if (banFileMonitorDto == null)
                {
                    if (!string.IsNullOrWhiteSpace(gameServerDto.FtpHostname) && !string.IsNullOrWhiteSpace(gameServerDto.FtpUsername) && !string.IsNullOrWhiteSpace(gameServerDto.FtpPassword) && gameServerDto.FtpPort != null)
                    {
                        logger.LogInformation($"BanFileMonitor for '{gameServerDto.Title}' does not exist - creating");

                        AsyncFtpClient? ftpClient = null;
                        try
                        {
                            ftpClient = new AsyncFtpClient(gameServerDto.FtpHostname, gameServerDto.FtpUsername, gameServerDto.FtpPassword, gameServerDto.FtpPort.Value, logger: new FtpLogAdapter(logger));
                            ftpClient.ValidateCertificate += (control, e) =>
                            {
                                if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                                { // Account for self-signed FTP certificate for self-hosted servers
                                    e.Accept = true;
                                }
                            };

                            await ftpClient.AutoConnect();

                            if (await ftpClient.FileExists($"/{gameServerDto.LiveMod}/ban.txt"))
                            {
                                var createBanFileMonitorDto = new CreateBanFileMonitorDto(gameServerDto.GameServerId, $"/{gameServerDto.LiveMod}/ban.txt", gameServerDto.GameType);
                                await repositoryApiClient.BanFileMonitors.CreateBanFileMonitor(createBanFileMonitorDto);
                            }
                        }
                        finally
                        {
                            ftpClient?.Dispose();
                        }
                    }
                }
                else
                {
                    if (!banFileMonitorDto.FilePath.ToLower().Contains(gameServerDto.LiveMod))
                    {
                        if (!string.IsNullOrWhiteSpace(gameServerDto.FtpHostname) && !string.IsNullOrWhiteSpace(gameServerDto.FtpUsername) && !string.IsNullOrWhiteSpace(gameServerDto.FtpPassword) && gameServerDto.FtpPort != null)
                        {
                            logger.LogInformation($"BanFileMonitor for '{gameServerDto.Title}' does not have current mod in path - updating");

                            AsyncFtpClient? ftpClient = null;
                            try
                            {
                                ftpClient = new AsyncFtpClient(gameServerDto.FtpHostname, gameServerDto.FtpUsername, gameServerDto.FtpPassword, gameServerDto.FtpPort.Value, logger: new FtpLogAdapter(logger));
                                ftpClient.ValidateCertificate += (control, e) =>
                                {
                                    if (e.Certificate.GetCertHashString().Equals(configuration["xtremeidiots_ftp_certificate_thumbprint"]))
                                    { // Account for self-signed FTP certificate for self-hosted servers
                                        e.Accept = true;
                                    }
                                };

                                await ftpClient.AutoConnect();

                                if (await ftpClient.DirectoryExists(gameServerDto.LiveMod))
                                {
                                    var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorDto.BanFileMonitorId, $"/{gameServerDto.LiveMod}/ban.txt");
                                    await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(editBanFileMonitorDto);
                                }
                            }
                            finally
                            {
                                ftpClient?.Dispose();
                            }
                        }
                    }
                }
            }
        }
    }
}
