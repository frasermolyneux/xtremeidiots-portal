using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.SyncFunc.Helpers;
using XtremeIdiots.Portal.SyncFunc.Interfaces;

namespace XtremeIdiots.Portal.SyncFunc
{
    public class BanFileMonitor
    {
        private readonly ILogger<BanFileMonitor> logger;
        private readonly IFtpHelper ftpHelper;
        private readonly IBanFileIngest banFileIngest;
        private readonly IBanFilesRepository banFilesRepository;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;

        public BanFileMonitor(
            ILogger<BanFileMonitor> logger,
            IFtpHelper ftpHelper,
            IBanFileIngest banFileIngest,
            IBanFilesRepository banFilesRepository,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ftpHelper = ftpHelper ?? throw new ArgumentNullException(nameof(ftpHelper));
            this.banFileIngest = banFileIngest ?? throw new ArgumentNullException(nameof(banFileIngest));
            this.banFilesRepository = banFilesRepository ?? throw new ArgumentNullException(nameof(banFilesRepository));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName("BanFileImportAndUpdate")]
        public async Task ImportLatestBanFiles([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            var banFileMonitorDtos = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(null, null, null, 0, 0, null);

            if (banFileMonitorDtos == null)
            {
                logger.LogCritical("Failed to retrieve ban file monitors from the repository");
                return;
            }

            foreach (var banFileMonitorDto in banFileMonitorDtos)
            {
                try
                {
                    var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitorDto.ServerId);

                    if (gameServerDto == null)
                    {
                        logger.LogError($"Failed to retrieve game server with id '{banFileMonitorDto.ServerId}' from the repository");
                        continue;
                    }

                    var remoteFileSize = await ftpHelper.GetFileSize(
                        gameServerDto.FtpHostname,
                        banFileMonitorDto.FilePath,
                        gameServerDto.FtpUsername,
                        gameServerDto.FtpPassword);

                    var banFileSize = await banFilesRepository.GetBanFileSizeForGame(gameServerDto.GameType.ToString());

                    if (remoteFileSize == null)
                    {
                        var telemetry = new EventTelemetry("BanFileInit");
                        telemetry.Properties.Add("GameType", gameServerDto.GameType.ToString());
                        telemetry.Properties.Add("ServerId", gameServerDto.Id.ToString());
                        telemetry.Properties.Add("ServerName", gameServerDto.Title);
                        telemetryClient.TrackEvent(telemetry);

                        var banFileStream = await banFilesRepository.GetBanFileForGame(gameServerDto.GameType.ToString());

                        await ftpHelper.UpdateRemoteFileFromStream(
                            gameServerDto.FtpHostname,
                            banFileMonitorDto.FilePath,
                            gameServerDto.FtpUsername,
                            gameServerDto.FtpPassword,
                            banFileStream);

                        banFileMonitorDto.RemoteFileSize = banFileSize;

                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitorDto);
                        continue;
                    }

                    if (remoteFileSize != banFileMonitorDto.RemoteFileSize)
                    {
                        var telemetry = new EventTelemetry("BanFileChangedOnRemote");
                        telemetry.Properties.Add("GameType", gameServerDto.GameType.ToString());
                        telemetry.Properties.Add("ServerId", gameServerDto.Id.ToString());
                        telemetry.Properties.Add("ServerName", gameServerDto.Title);
                        telemetryClient.TrackEvent(telemetry);

                        var remoteBanFileData = await ftpHelper.GetRemoteFileData(
                            gameServerDto.FtpHostname,
                            banFileMonitorDto.FilePath,
                            gameServerDto.FtpUsername,
                            gameServerDto.FtpPassword);

                        await banFileIngest.IngestBanFileDataForGame(gameServerDto.GameType.ToString(), remoteBanFileData);

                        banFileMonitorDto.RemoteFileSize = (long)remoteFileSize;

                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitorDto);
                    }

                    if (remoteFileSize != banFileSize && remoteFileSize == banFileMonitorDto.RemoteFileSize)
                    {
                        var telemetry = new EventTelemetry("BanFileChangedOnSource");
                        telemetry.Properties.Add("GameType", gameServerDto.GameType.ToString());
                        telemetry.Properties.Add("ServerId", gameServerDto.Id.ToString());
                        telemetry.Properties.Add("ServerName", gameServerDto.Title);
                        telemetryClient.TrackEvent(telemetry);

                        var banFileStream = await banFilesRepository.GetBanFileForGame(gameServerDto.GameType.ToString());

                        await ftpHelper.UpdateRemoteFileFromStream(
                            gameServerDto.FtpHostname,
                            banFileMonitorDto.FilePath,
                            gameServerDto.FtpUsername,
                            gameServerDto.FtpPassword,
                            banFileStream);

                        banFileMonitorDto.RemoteFileSize = banFileSize;

                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitorDto);
                    }

                    banFileMonitorDto.LastSync = DateTime.UtcNow;
                    await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitorDto);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to process 'BanFileImportAndUpdate' for id '{banFileMonitorDto.ServerId}'");
                }
            }
        }

        [FunctionName("GenerateLatestBansFile")]
        public async Task GenerateLatestBansFile([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start GenerateLatestBansFile @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var gameType in new string[] { "CallOfDuty2", "CallOfDuty4", "CallOfDuty5" })
                try
                {
                    await banFilesRepository.RegenerateBanFileForGame(gameType);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to regenerate latest ban file for {game}", gameType);
                }


            stopWatch.Stop();
            log.LogDebug($"Stop GenerateLatestBansFile @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}