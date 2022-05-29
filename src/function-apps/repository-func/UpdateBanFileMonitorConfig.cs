using FluentFTP;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
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
            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, GameServerFilter.LiveStatusEnabled, 0, 0, null);
            var banFileMonitorDtos = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(gameTypes, null, null, 0, 0, null);

            if (gameServerDtos == null)
            {
                logger.LogCritical("Failed to retrieve game servers from repository");
                return;
            }

            if (banFileMonitorDtos == null)
            {
                logger.LogCritical("Failed to retrieve ban file monitors from repository");
                return;
            }

            foreach (var gameServerDto in gameServerDtos)
            {
                if (string.IsNullOrWhiteSpace(gameServerDto.LiveMod))
                    continue;

                var banFileMonitorDto = banFileMonitorDtos.SingleOrDefault(bfm => bfm.ServerId == gameServerDto.Id);

                if (banFileMonitorDto == null)
                {
                    if (!string.IsNullOrWhiteSpace(gameServerDto.FtpHostname) && !string.IsNullOrWhiteSpace(gameServerDto.FtpUsername) && !string.IsNullOrWhiteSpace(gameServerDto.FtpPassword))
                    {
                        logger.LogInformation($"BanFileMonitor for '{gameServerDto.Title}' does not exist - creating");

                        FtpClient? ftpClient = null;
                        try
                        {
                            ftpClient = new FtpClient(gameServerDto.FtpHostname, gameServerDto.FtpPort, gameServerDto.FtpUsername, gameServerDto.FtpPassword);
                            ftpClient.AutoConnect();

                            if (await ftpClient.FileExistsAsync($"/{gameServerDto.LiveMod}/ban.txt"))
                            {
                                banFileMonitorDto = new BanFileMonitorDto
                                {
                                    GameType = gameServerDto.GameType,
                                    FilePath = $"/{gameServerDto.LiveMod}/ban.txt"
                                };

                                await repositoryApiClient.GameServers.CreateBanFileMonitorForGameServer(gameServerDto.Id, banFileMonitorDto);
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
                                ftpClient.AutoConnect();

                                if (await ftpClient.FileExistsAsync($"/{gameServerDto.LiveMod}/ban.txt"))
                                {
                                    banFileMonitorDto.FilePath = $"/{gameServerDto.LiveMod}/ban.txt";
                                    await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitorDto);
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
