using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Integrations.Interfaces;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class FtpFileMonitor
    {
        private readonly IFileMonitorsRepository _fileMonitorsRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly ILogFileMonitorStateRepository _logFileMonitorStateRepository;
        private readonly IServiceProvider _serviceProvider;

        public FtpFileMonitor(
            IFileMonitorsRepository fileMonitorsRepository,
            IGameServerStatusRepository gameServerStatusRepository,
            ILogFileMonitorStateRepository logFileMonitorStateRepository,
            IServiceProvider serviceProvider)
        {
            _fileMonitorsRepository = fileMonitorsRepository ?? throw new ArgumentNullException(nameof(fileMonitorsRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _logFileMonitorStateRepository = logFileMonitorStateRepository ?? throw new ArgumentNullException(nameof(logFileMonitorStateRepository));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        [FunctionName("SyncLogFileMonitorState")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunSyncLogFileMonitorState([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunSyncLogFileMonitorState @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fileMonitors = await _fileMonitorsRepository.GetFileMonitors(new FileMonitorFilterModel());
            var fileMonitorStates = await _logFileMonitorStateRepository.GetLogFileMonitorStates();

            foreach (var fileMonitorDto in fileMonitors)
            {
                var fileMonitorState = fileMonitorStates.SingleOrDefault(fm => fm.FileMonitorId == fileMonitorDto.FileMonitorId);

                if (fileMonitorState == null)
                {
                    await _logFileMonitorStateRepository.UpdateState(new LogFileMonitorStateDto
                    {
                        FileMonitorId = fileMonitorDto.FileMonitorId,
                        ServerId = fileMonitorDto.ServerId,
                        GameType = fileMonitorDto.GameServer.GameType,
                        ServerTitle = fileMonitorDto.GameServer.Title,
                        FilePath = fileMonitorDto.FilePath,
                        FtpHostname = fileMonitorDto.GameServer.FtpHostname,
                        FtpUsername = fileMonitorDto.GameServer.FtpUsername,
                        FtpPassword = fileMonitorDto.GameServer.FtpPassword,
                        RemoteSize = -1,
                        LastReadAttempt = DateTime.UtcNow,
                        LastRead = DateTime.UtcNow
                    });

                    log.LogInformation($"Creating new log file monitor state object for {fileMonitorDto.GameServer.Title} against path {fileMonitorDto.FilePath}");
                }
                else
                {
                    fileMonitorState.ServerTitle = fileMonitorDto.GameServer.Title;

                    if (fileMonitorState.FilePath != fileMonitorDto.FilePath)
                    {
                        fileMonitorState.FilePath = fileMonitorDto.FilePath;
                        fileMonitorState.RemoteSize = -1;
                        fileMonitorState.LastReadAttempt = DateTime.UtcNow;
                        fileMonitorState.LastRead = DateTime.UtcNow;
                    }

                    fileMonitorState.FtpHostname = fileMonitorDto.GameServer.FtpHostname;
                    fileMonitorState.FtpUsername = fileMonitorDto.GameServer.FtpUsername;
                    fileMonitorState.FtpPassword = fileMonitorDto.GameServer.FtpPassword;

                    await _logFileMonitorStateRepository.UpdateState(fileMonitorState);
                }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunSyncLogFileMonitorState @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        [FunctionName("MonitorLogFileOrchestrator")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunMonitorLogFileOrchestrator([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunMonitorLogFileOrchestrator @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fileMonitorStates = await _logFileMonitorStateRepository.GetLogFileMonitorStates();
            var gameServerStatus = await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero);

            log.LogDebug($"Processing {fileMonitorStates.Count} file monitor states");

            foreach (var fileMonitorStateDto in fileMonitorStates)
            {
                var statusModel = gameServerStatus.SingleOrDefault(s => s.ServerId == fileMonitorStateDto.ServerId);

                if (statusModel == null)
                {
                    log.LogWarning($"There is no game server status model for {fileMonitorStateDto.ServerTitle}");
                    continue;
                }

                if (statusModel.PlayerCount > 0)
                {
                    fileMonitorStateDto.LastReadAttempt = DateTime.UtcNow;
                    await _logFileMonitorStateRepository.UpdateState(fileMonitorStateDto);

                    var requestPath = $"ftp://{fileMonitorStateDto.FtpHostname}{fileMonitorStateDto.FilePath}";
                    log.LogDebug($"Performing request for {fileMonitorStateDto.ServerTitle} against file {requestPath} as player count is {statusModel.PlayerCount}");
                    log.LogInformation("TODO - Call a function that will perform the FTP request");
                }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunMonitorLogFileOrchestrator @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        private static long GetFileSize(string username, string password, string requestPath)
        {
            var request = (FtpWebRequest) WebRequest.Create(requestPath);
            request.KeepAlive = true;
            request.UsePassive = false;
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            var fileSize = ((FtpWebResponse) request.GetResponse()).ContentLength;
            return fileSize;
        }
    }
}