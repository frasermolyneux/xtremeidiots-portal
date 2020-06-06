using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using XI.Portal.Servers.Dto;
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

        public FtpFileMonitor(
            IFileMonitorsRepository fileMonitorsRepository, 
            IGameServerStatusRepository gameServerStatusRepository,
            ILogFileMonitorStateRepository logFileMonitorStateRepository)
        {
            _fileMonitorsRepository = fileMonitorsRepository ?? throw new ArgumentNullException(nameof(fileMonitorsRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _logFileMonitorStateRepository = logFileMonitorStateRepository ?? throw new ArgumentNullException(nameof(logFileMonitorStateRepository));
        }

        [FunctionName("SyncLogFileMonitorState")]
        public async Task SyncLogFileMonitorState([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Start SyncLogFileMonitorState @ {DateTime.Now}");

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
                        LastRead = DateTime.UtcNow
                    });
                }
                else
                {
                    fileMonitorState.ServerTitle = fileMonitorDto.GameServer.Title;
                    fileMonitorState.FilePath = fileMonitorDto.FilePath;
                    fileMonitorState.FtpHostname = fileMonitorDto.GameServer.FtpHostname;
                    fileMonitorState.FtpUsername = fileMonitorDto.GameServer.FtpUsername;
                    fileMonitorState.FtpPassword = fileMonitorDto.GameServer.FtpPassword;

                    await _logFileMonitorStateRepository.UpdateState(fileMonitorState);
                }
            }

            stopWatch.Stop();
            log.LogInformation($"Stop SyncLogFileMonitorState @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        [FunctionName("MonitorLogFile")]
        public async Task MonitorLogFile([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Start MonitorLogFile @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fileMonitorStates = await _logFileMonitorStateRepository.GetLogFileMonitorStates();
            var gameServerStatus = await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero);

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
                    var requestPath = $"ftp://{fileMonitorStateDto.FtpHostname}{fileMonitorStateDto.FilePath}";
                    log.LogInformation($"Performing request for {fileMonitorStateDto.ServerTitle} against file {requestPath} as player count is {statusModel.PlayerCount}");

                    if (fileMonitorStateDto.RemoteSize == -1 || fileMonitorStateDto.LastRead < DateTime.UtcNow.AddMinutes(-5))
                    {
                        log.LogWarning($"The remote file for {fileMonitorStateDto.ServerTitle} ({requestPath}) has not been read in five minutes");

                        var fileSize = GetFileSize(fileMonitorStateDto.FtpUsername, fileMonitorStateDto.FtpPassword, requestPath);
                        log.LogInformation($"The remote file size for {fileMonitorStateDto.ServerTitle} is {fileSize} bytes");

                        fileMonitorStateDto.LastRead = DateTime.UtcNow;
                        fileMonitorStateDto.RemoteSize = fileSize;

                        await _logFileMonitorStateRepository.UpdateState(fileMonitorStateDto);
                    }
                    else
                    {
                        log.LogInformation("TODO - Read offset of file here and process the new data");
                    }
                }
                else
                {
                    log.LogDebug($"Skipping monitor as the player count is 0 for {fileMonitorStateDto.ServerTitle}");
                }
            }

            stopWatch.Stop();
            log.LogInformation($"Stop MonitorLogFile @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
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