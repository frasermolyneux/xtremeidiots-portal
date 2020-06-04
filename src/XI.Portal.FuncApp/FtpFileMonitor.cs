using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class FtpFileMonitor
    {
        private readonly IFileMonitorsRepository _fileMonitorsRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;


        public FtpFileMonitor(IFileMonitorsRepository fileMonitorsRepository, IGameServerStatusRepository gameServerStatusRepository)
        {
            _fileMonitorsRepository = fileMonitorsRepository ?? throw new ArgumentNullException(nameof(fileMonitorsRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
        }

        [FunctionName("MonitorLogFile")]
        public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Starting monitor of log file: {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fileMonitors = await _fileMonitorsRepository.GetFileMonitors(new FileMonitorFilterModel());
            var gameServerStatus = await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero);

            foreach (var fileMonitorDto in fileMonitors)
            {
                var statusModel = gameServerStatus.SingleOrDefault(s => s.ServerId == fileMonitorDto.ServerId);

                if (statusModel == null)
                {
                    log.LogWarning($"There is no game server status model for {fileMonitorDto.GameServer.Title}");
                    continue;
                }

                if (statusModel.PlayerCount > 0)
                {
                    var requestPath = $"ftp://{fileMonitorDto.GameServer.FtpHostname}{fileMonitorDto.FilePath}";
                    log.LogInformation($"Performing request for {fileMonitorDto.GameServer.Title} against file {requestPath}");

                    var fileSize = GetFileSize(fileMonitorDto.GameServer.FtpUsername, fileMonitorDto.GameServer.FtpPassword, requestPath);
                    log.LogInformation($"The remote file size is {fileSize} bytes");
                }
                else
                {
                    log.LogDebug($"Skipping monitor as the player count is 0 for {fileMonitorDto.GameServer.Title}");
                }
            }

            stopWatch.Stop();
            log.LogInformation($"Completed after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        private static long GetFileSize(string username, string password, string requestPath)
        {
            var request = (FtpWebRequest)WebRequest.Create(requestPath);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            var fileSize = ((FtpWebResponse)request.GetResponse()).ContentLength;
            return fileSize;
        }
    }
}