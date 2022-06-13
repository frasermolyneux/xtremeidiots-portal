using FluentFTP;

using Microsoft.Azure.WebJobs;
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

        public UpdateBanFileMonitorConfig(
            ILogger<UpdateBanFileMonitorConfig> logger,
            IRepositoryApiClient repositoryApiClient)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
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
                    if (!string.IsNullOrWhiteSpace(gameServerDto.FtpHostname) && !string.IsNullOrWhiteSpace(gameServerDto.FtpUsername) && !string.IsNullOrWhiteSpace(gameServerDto.FtpPassword))
                    {
                        logger.LogInformation($"BanFileMonitor for '{gameServerDto.Title}' does not exist - creating");

                        FtpClient? ftpClient = null;
                        try
                        {
                            ftpClient = new FtpClient(gameServerDto.FtpHostname, gameServerDto.FtpPort, gameServerDto.FtpUsername, gameServerDto.FtpPassword);
                            ftpClient.ValidateAnyCertificate = true;
                            ftpClient.AutoConnect();

                            if (await ftpClient.FileExistsAsync($"/{gameServerDto.LiveMod}/ban.txt"))
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
                        if (!string.IsNullOrWhiteSpace(gameServerDto.FtpHostname) && !string.IsNullOrWhiteSpace(gameServerDto.FtpUsername) && !string.IsNullOrWhiteSpace(gameServerDto.FtpPassword))
                        {
                            logger.LogInformation($"BanFileMonitor for '{gameServerDto.Title}' does not have current mod in path - updating");

                            FtpClient? ftpClient = null;
                            try
                            {
                                ftpClient = new FtpClient(gameServerDto.FtpHostname, gameServerDto.FtpPort, gameServerDto.FtpUsername, gameServerDto.FtpPassword);
                                ftpClient.ValidateAnyCertificate = true;
                                ftpClient.AutoConnect();

                                if (await ftpClient.DirectoryExistsAsync(gameServerDto.LiveMod))
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
