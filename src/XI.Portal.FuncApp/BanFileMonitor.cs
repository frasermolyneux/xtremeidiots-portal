using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Utilities.FtpHelper;

namespace XI.Portal.FuncApp
{
    public class BanFileMonitor
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IFtpHelper _ftpHelper;
        private readonly IBanFileIngest _banFileIngest;
        private readonly ILogger<BanFileMonitor> _logger;

        public BanFileMonitor(
            ILogger<BanFileMonitor> logger,
            IBanFileMonitorsRepository banFileMonitorsRepository,
            IFtpHelper ftpHelper,
            IBanFileIngest banFileIngest)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
            _ftpHelper = ftpHelper ?? throw new ArgumentNullException(nameof(ftpHelper));
            _banFileIngest = banFileIngest;
        }

        [FunctionName("ImportLatestBanFiles")]
        public async Task ImportLatestBanFiles([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start ImportLatestBanFiles @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var banFileMonitors = await _banFileMonitorsRepository.GetBanFileMonitors(new BanFileMonitorFilterModel());
            foreach (var banFileMonitor in banFileMonitors)
                try
                {
                    _logger.LogDebug("Importing latest ban file for {server}", banFileMonitor.GameServer.Title);

                    var remoteFileSize = _ftpHelper.GetFileSize(
                        banFileMonitor.GameServer.FtpHostname, 
                        banFileMonitor.FilePath, 
                        banFileMonitor.GameServer.FtpUsername,
                        banFileMonitor.GameServer.FtpPassword);

                    if (remoteFileSize != banFileMonitor.RemoteFileSize)
                    {
                        _logger.LogInformation("Remote ban file on {server} has changed since last sync: {current} != {last}", banFileMonitor.GameServer.Title, remoteFileSize, banFileMonitor.RemoteFileSize);

                        var remoteBanFileData = _ftpHelper.GetRemoteFileData(
                            banFileMonitor.GameServer.FtpHostname, 
                            banFileMonitor.FilePath, 
                            banFileMonitor.GameServer.FtpUsername,
                            banFileMonitor.GameServer.FtpPassword);

                        await _banFileIngest.IngestBanFileDataForGame(banFileMonitor.GameServer.GameType, remoteBanFileData);

                        banFileMonitor.RemoteFileSize = remoteFileSize;

                        await _banFileMonitorsRepository.UpdateBanFileMonitor(banFileMonitor);
                    }
                    else
                        _logger.LogDebug("Remote ban file on {server} has not been modified since last sync", banFileMonitor.GameServer.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import latest ban file for {server}", banFileMonitor.GameServer.Title);
                }


            stopWatch.Stop();
            log.LogDebug($"Stop ImportLatestBanFiles @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}