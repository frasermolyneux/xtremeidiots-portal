using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.SyncFunc.FtpHelper;
using XtremeIdiots.Portal.SyncFunc.Interfaces;

namespace XtremeIdiots.Portal.SyncFunc
{
    public class BanFileMonitor
    {
        private readonly IBanFileIngest _banFileIngest;
        private readonly IBanFilesRepository _banFilesRepository;

        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IFtpHelper _ftpHelper;
        private readonly ILogger<BanFileMonitor> _logger;

        public BanFileMonitor(
            ILogger<BanFileMonitor> logger,
            IFtpHelper ftpHelper,
            IBanFileIngest banFileIngest,
            IBanFilesRepository banFilesRepository,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ftpHelper = ftpHelper ?? throw new ArgumentNullException(nameof(ftpHelper));
            _banFileIngest = banFileIngest;
            _banFilesRepository = banFilesRepository ?? throw new ArgumentNullException(nameof(banFilesRepository));

            this.repositoryApiClient = repositoryApiClient;
        }

        [FunctionName("BanFileImportAndUpdate")]
        public async Task ImportLatestBanFiles([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start BanFileImportAndUpdate @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();


            var banFileMonitors = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(null, null, null, 0, 0, null);

            foreach (var banFileMonitor in banFileMonitors)
            {
                try
                {
                    var server = await repositoryApiClient.GameServers.GetGameServer(banFileMonitor.ServerId);

                    _logger.LogDebug("Checking ban file for {server}", server.Title);

                    var remoteFileSize = _ftpHelper.GetFileSize(
                        server.FtpHostname,
                        banFileMonitor.FilePath,
                        server.FtpUsername,
                        server.FtpPassword);

                    var banFileSize = await _banFilesRepository.GetBanFileSizeForGame(server.GameType.ToString());

                    if (remoteFileSize == 0)
                    {
                        _logger.LogInformation("Remote ban file on {server} at {path} is zero - updating file", server.Title, banFileMonitor.FilePath);

                        var banFileStream = await _banFilesRepository.GetBanFileForGame(server.GameType.ToString());

                        await _ftpHelper.UpdateRemoteFileFromStream(
                            server.FtpHostname,
                            banFileMonitor.FilePath,
                            server.FtpUsername,
                            server.FtpPassword,
                            banFileStream);

                        banFileMonitor.RemoteFileSize = banFileSize;

                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitor);
                        continue;
                    }

                    if (remoteFileSize != banFileMonitor.RemoteFileSize)
                    {
                        _logger.LogInformation("Remote ban file on {server} at {path} has changed since last sync: {current} != {last}", server.Title, banFileMonitor.FilePath, remoteFileSize, banFileMonitor.RemoteFileSize);

                        var remoteBanFileData = _ftpHelper.GetRemoteFileData(
                            server.FtpHostname,
                            banFileMonitor.FilePath,
                            server.FtpUsername,
                            server.FtpPassword);

                        await _banFileIngest.IngestBanFileDataForGame(server.GameType.ToString(), remoteBanFileData);

                        banFileMonitor.RemoteFileSize = remoteFileSize;

                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitor);
                    }
                    else
                    {
                        _logger.LogDebug("Remote ban file on {server} at {path} has not been modified since last sync", server.Title, banFileMonitor.FilePath);
                    }

                    if (remoteFileSize != banFileSize && remoteFileSize == banFileMonitor.RemoteFileSize)
                    {
                        _logger.LogInformation("Remote ban file on {server} at {path} is not the latest version", server.Title, banFileMonitor.FilePath);

                        var banFileStream = await _banFilesRepository.GetBanFileForGame(server.GameType.ToString());

                        await _ftpHelper.UpdateRemoteFileFromStream(
                            server.FtpHostname,
                            banFileMonitor.FilePath,
                            server.FtpUsername,
                            server.FtpPassword,
                            banFileStream);

                        banFileMonitor.RemoteFileSize = banFileSize;

                        await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitor);
                    }
                    else
                    {
                        _logger.LogDebug("Remote ban file on {server} at {path} has the latest ban file", server.Title, banFileMonitor.FilePath);
                    }

                    banFileMonitor.LastSync = DateTime.UtcNow;
                    await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitor);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to check ban file for {server}", banFileMonitor.ServerId);
                }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop BanFileImportAndUpdate @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
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
                    await _banFilesRepository.RegenerateBanFileForGame(gameType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to regenerate latest ban file for {game}", gameType);
                }


            stopWatch.Stop();
            log.LogDebug($"Stop GenerateLatestBansFile @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}