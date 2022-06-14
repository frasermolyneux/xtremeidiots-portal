using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using System.Diagnostics;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
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
            var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(null, null, null, 0, 50, null);

            if (!banFileMonitorsApiResponse.IsSuccess || banFileMonitorsApiResponse.Result == null)
            {
                logger.LogCritical("Failed to retrieve ban file monitors from the repository");
                return;
            }

            foreach (var banFileMonitorDto in banFileMonitorsApiResponse.Result.Entries)
            {
                if (banFileMonitorDto.GameServer == null)
                    continue;

                if (string.IsNullOrWhiteSpace(banFileMonitorDto.GameServer.FtpHostname)
                    || string.IsNullOrWhiteSpace(banFileMonitorDto.GameServer.FtpUsername)
                    || string.IsNullOrWhiteSpace(banFileMonitorDto.GameServer.FtpPassword)
                    || banFileMonitorDto.GameServer.FtpPort == null)
                    continue;

                try
                {
                    var remoteFileSize = await ftpHelper.GetFileSize(
                    banFileMonitorDto.GameServer.FtpHostname,
                    banFileMonitorDto.GameServer.FtpPort.Value,
                    banFileMonitorDto.FilePath,
                    banFileMonitorDto.GameServer.FtpUsername,
                    banFileMonitorDto.GameServer.FtpPassword);
                    var banFileSize = await banFilesRepository.GetBanFileSizeForGame(banFileMonitorDto.GameServer.GameType);

                    if (remoteFileSize == null)
                    {
                        var telemetry = new EventTelemetry("BanFileInit");
                        telemetry.Properties.Add("GameType", banFileMonitorDto.GameServer.GameType.ToString());
                        telemetry.Properties.Add("GameServerId", banFileMonitorDto.GameServer.GameServerId.ToString());
                        telemetry.Properties.Add("GameServerName", banFileMonitorDto.GameServer.Title);
                        telemetryClient.TrackEvent(telemetry);

                        var banFileStream = await banFilesRepository.GetBanFileForGame(banFileMonitorDto.GameServer.GameType);

                        await ftpHelper.UpdateRemoteFileFromStream(
                            banFileMonitorDto.GameServer.FtpHostname,
                            banFileMonitorDto.GameServer.FtpPort.Value,
                            banFileMonitorDto.FilePath,
                            banFileMonitorDto.GameServer.FtpUsername,
                            banFileMonitorDto.GameServer.FtpPassword,
                            banFileStream);

                        var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorDto.BanFileMonitorId, banFileSize, DateTime.UtcNow);
                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(editBanFileMonitorDto);
                        continue;
                    }

                    if (remoteFileSize != banFileMonitorDto.RemoteFileSize)
                    {
                        var telemetry = new EventTelemetry("BanFileChangedOnRemote");
                        telemetry.Properties.Add("GameType", banFileMonitorDto.GameServer.GameType.ToString());
                        telemetry.Properties.Add("GameServerId", banFileMonitorDto.GameServer.GameServerId.ToString());
                        telemetry.Properties.Add("GameServerName", banFileMonitorDto.GameServer.Title);
                        telemetryClient.TrackEvent(telemetry);

                        var remoteBanFileData = await ftpHelper.GetRemoteFileData(
                            banFileMonitorDto.GameServer.FtpHostname,
                            banFileMonitorDto.GameServer.FtpPort.Value,
                            banFileMonitorDto.FilePath,
                            banFileMonitorDto.GameServer.FtpUsername,
                            banFileMonitorDto.GameServer.FtpPassword);

                        await banFileIngest.IngestBanFileDataForGame(banFileMonitorDto.GameServer.GameType.ToString(), remoteBanFileData);

                        var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorDto.BanFileMonitorId, (long)remoteFileSize, DateTime.UtcNow);
                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(editBanFileMonitorDto);
                    }

                    if (remoteFileSize != banFileSize && remoteFileSize == banFileMonitorDto.RemoteFileSize)
                    {
                        var telemetry = new EventTelemetry("BanFileChangedOnSource");
                        telemetry.Properties.Add("GameType", banFileMonitorDto.GameServer.GameType.ToString());
                        telemetry.Properties.Add("GameServerId", banFileMonitorDto.GameServer.GameServerId.ToString());
                        telemetry.Properties.Add("GameServerName", banFileMonitorDto.GameServer.Title);
                        telemetryClient.TrackEvent(telemetry);

                        var banFileStream = await banFilesRepository.GetBanFileForGame(banFileMonitorDto.GameServer.GameType);

                        await ftpHelper.UpdateRemoteFileFromStream(
                            banFileMonitorDto.GameServer.FtpHostname,
                            banFileMonitorDto.GameServer.FtpPort.Value,
                            banFileMonitorDto.FilePath,
                            banFileMonitorDto.GameServer.FtpUsername,
                            banFileMonitorDto.GameServer.FtpPassword,
                            banFileStream);

                        var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorDto.BanFileMonitorId, banFileSize, DateTime.UtcNow);
                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(editBanFileMonitorDto);
                    }

                    if (remoteFileSize == banFileMonitorDto.RemoteFileSize)
                    {
                        var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorDto.BanFileMonitorId)
                        {
                            LastSync = DateTime.UtcNow
                        };
                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(editBanFileMonitorDto);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to process 'BanFileImportAndUpdate' for id '{banFileMonitorDto.GameServerId}'");
                }
            }
        }

        [FunctionName("GenerateLatestBansFile")]
        public async Task GenerateLatestBansFile([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start GenerateLatestBansFile @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var gameType in new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5 })
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