using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Players.Constants;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Utilities.FtpHelper;

namespace XI.Portal.FuncApp
{
    public class BanFileMonitor
    {
        private readonly IBanFileIngest _banFileIngest;
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IBanFilesRepository _banFilesRepository;
        private readonly IFtpHelper _ftpHelper;
        private readonly ILogger<BanFileMonitor> _logger;

        public BanFileMonitor(
            ILogger<BanFileMonitor> logger,
            IBanFileMonitorsRepository banFileMonitorsRepository,
            IFtpHelper ftpHelper,
            IBanFileIngest banFileIngest,
            IBanFilesRepository banFilesRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
            _ftpHelper = ftpHelper ?? throw new ArgumentNullException(nameof(ftpHelper));
            _banFileIngest = banFileIngest;
            _banFilesRepository = banFilesRepository ?? throw new ArgumentNullException(nameof(banFilesRepository));
        }

        [FunctionName("ImportLatestBanFiles")]
        public async Task ImportLatestBanFiles([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start ImportLatestBanFiles @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var banFileMonitors = await _banFileMonitorsRepository.GetBanFileMonitors(new BanFileMonitorFilterModel());
            foreach (var banFileMonitor in banFileMonitors)
                try
                {
                    _logger.LogDebug("Importing latest ban file for {server}", banFileMonitor.GameServer.Title);

                    var remoteFileSize = _ftpHelper.GetFileSize(
                        banFileMonitor.GameServer.FtpHostname,
                        banFileMonitor.FilePath + ".test",
                        banFileMonitor.GameServer.FtpUsername,
                        banFileMonitor.GameServer.FtpPassword);

                    if (remoteFileSize == 0)
                    {
                        banFileMonitor.RemoteFileSize = remoteFileSize;
                        await _banFileMonitorsRepository.UpdateBanFileMonitor(banFileMonitor);
                        continue;
                    }

                    if (remoteFileSize != banFileMonitor.RemoteFileSize)
                    {
                        _logger.LogInformation("Remote ban file on {server} has changed since last sync: {current} != {last}", banFileMonitor.GameServer.Title, remoteFileSize, banFileMonitor.RemoteFileSize);

                        var remoteBanFileData = _ftpHelper.GetRemoteFileData(
                            banFileMonitor.GameServer.FtpHostname,
                            banFileMonitor.FilePath + ".test",
                            banFileMonitor.GameServer.FtpUsername,
                            banFileMonitor.GameServer.FtpPassword);

                        await _banFileIngest.IngestBanFileDataForGame(banFileMonitor.GameServer.GameType, remoteBanFileData);

                        banFileMonitor.RemoteFileSize = remoteFileSize;

                        await _banFileMonitorsRepository.UpdateBanFileMonitor(banFileMonitor);
                    }
                    else
                    {
                        _logger.LogDebug("Remote ban file on {server} has not been modified since last sync", banFileMonitor.GameServer.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import latest ban file for {server}", banFileMonitor.GameServer.Title);
                }

            stopWatch.Stop();
            log.LogDebug($"Stop ImportLatestBanFiles @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        [FunctionName("GenerateLatestBansFile")]
        public async Task GenerateLatestBansFile([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start GenerateLatestBansFile @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var gameType in BanFilesSupportedGames.Games)
                try
                {
                    await _banFilesRepository.RegenerateBanFileForGame(gameType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to regenerate latest ban file for {game}", gameType);
                }


            stopWatch.Stop();
            log.LogDebug($"Stop GenerateLatestBansFile @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        [FunctionName("UpdateRemotesWithLatestBanFiles")]
        public async Task UpdateRemotesWithLatestBanFiles([TimerTrigger("30 */10 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start UpdateRemotesWithLatestBanFiles @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var banFileMonitors = await _banFileMonitorsRepository.GetBanFileMonitors(new BanFileMonitorFilterModel());
            foreach (var banFileMonitor in banFileMonitors)
                try
                {
                    _logger.LogDebug("Updating {server} with latest ban file", banFileMonitor.GameServer.Title);

                    var remoteFileSize = _ftpHelper.GetFileSize(
                        banFileMonitor.GameServer.FtpHostname,
                        banFileMonitor.FilePath + ".test",
                        banFileMonitor.GameServer.FtpUsername,
                        banFileMonitor.GameServer.FtpPassword);

                    var banFileSize = await _banFilesRepository.GetBanFileSizeForGame(banFileMonitor.GameServer.GameType);

                    if (remoteFileSize != banFileSize && remoteFileSize == banFileMonitor.RemoteFileSize)
                    {
                        _logger.LogInformation("Remote ban file on {server} at {path} is not the latest version", banFileMonitor.GameServer.Title, banFileMonitor.FilePath);

                        var banFileStream = await _banFilesRepository.GetBanFileForGame(banFileMonitor.GameServer.GameType);

                        await _ftpHelper.UpdateRemoteFileFromStream(
                            banFileMonitor.GameServer.FtpHostname,
                            banFileMonitor.FilePath + ".test",
                            banFileMonitor.GameServer.FtpUsername,
                            banFileMonitor.GameServer.FtpPassword,
                            banFileStream);

                        banFileMonitor.RemoteFileSize = banFileSize;

                        await _banFileMonitorsRepository.UpdateBanFileMonitor(banFileMonitor);
                    }
                    else
                    {
                        _logger.LogDebug("Remote ban file on {server} at {path} has the latest ban file", banFileMonitor.GameServer.Title, banFileMonitor.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update {server} with the latest ban file", banFileMonitor.GameServer.Title);
                }

            stopWatch.Stop();
            log.LogDebug($"Stop UpdateRemotesWithLatestBanFiles @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}