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
        public async Task RunSyncLogFileMonitorState([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunSyncLogFileMonitorState @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fileMonitors = await _fileMonitorsRepository.GetFileMonitors(new FileMonitorFilterModel());
            var fileMonitorStates = await _logFileMonitorStateRepository.GetLogFileMonitorStates();
            var gameServerStatus = await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero);

            foreach (var fileMonitorDto in fileMonitors)
            {
                var fileMonitorState = fileMonitorStates.SingleOrDefault(fm => fm.FileMonitorId == fileMonitorDto.FileMonitorId);
                var statusModel = gameServerStatus.SingleOrDefault(s => s.ServerId == fileMonitorDto.ServerId);

                var playerCount = 0;
                if (statusModel != null)
                    playerCount = statusModel.PlayerCount;

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
                        LastRead = DateTime.UtcNow,
                        PlayerCount = playerCount
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

                    fileMonitorState.PlayerCount = playerCount;

                    await _logFileMonitorStateRepository.UpdateState(fileMonitorState);
                }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunSyncLogFileMonitorState @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        [FunctionName("MonitorLogFile")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunMonitorLogFile([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunMonitorLogFile @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var fileMonitorStates = (await _logFileMonitorStateRepository.GetLogFileMonitorStates())
                .Where(fm => fm.PlayerCount > 0)
                .ToList();

            if (!fileMonitorStates.Any())
                return;

            var selectedLogFileMonitor = fileMonitorStates.OrderBy(fm => fm.LastReadAttempt).First();
            selectedLogFileMonitor.LastReadAttempt = DateTime.UtcNow;
            await _logFileMonitorStateRepository.UpdateState(selectedLogFileMonitor);

            var requestPath = $"ftp://{selectedLogFileMonitor.FtpHostname}{selectedLogFileMonitor.FilePath}";
            log.LogDebug($"Performing request for {selectedLogFileMonitor.ServerTitle} against file {requestPath} as player count is {selectedLogFileMonitor.PlayerCount}");

            if (selectedLogFileMonitor.RemoteSize == -1 || selectedLogFileMonitor.LastRead < DateTime.UtcNow.AddMinutes(-1))
            {
                log.LogDebug($"The remote file for {selectedLogFileMonitor.ServerTitle} ({requestPath}) has not been read in the past minute");

                var fileSize = GetFileSize(selectedLogFileMonitor.FtpUsername, selectedLogFileMonitor.FtpPassword, requestPath);
                log.LogDebug($"The remote file size for {selectedLogFileMonitor.ServerTitle} is {fileSize} bytes");

                selectedLogFileMonitor.LastRead = DateTime.UtcNow;
                selectedLogFileMonitor.RemoteSize = fileSize;

                await _logFileMonitorStateRepository.UpdateState(selectedLogFileMonitor);
            }
            else
            {
                try
                {
                    var request = (FtpWebRequest) WebRequest.Create(requestPath);
                    request.KeepAlive = false;
                    request.UsePassive = true;
                    request.Credentials = new NetworkCredential(selectedLogFileMonitor.FtpUsername, selectedLogFileMonitor.FtpPassword);
                    request.ContentOffset = selectedLogFileMonitor.RemoteSize;
                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                    using var response = await request.GetResponseAsync();
                    using var streamReader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException());
                    var prev = -1;

                    var byteList = new List<byte>();

                    while (true)
                    {
                        var cur = streamReader.Read();

                        if (cur == -1) break;

                        byteList.Add((byte) cur);

                        if (prev == '\r' && cur == '\n')
                        {
                            selectedLogFileMonitor.RemoteSize += byteList.Count;
                            selectedLogFileMonitor.LastRead = DateTime.UtcNow;

                            await _logFileMonitorStateRepository.UpdateState(selectedLogFileMonitor);

                            var line = Encoding.UTF8.GetString(byteList.ToArray()).TrimEnd('\n');
                            try
                            {
                                line = line.Replace("\r\n", "");
                                line = line.Trim();
                                line = line.Substring(line.IndexOf(' ') + 1);
                                line = line.Replace("\u0015", "");

                                if (line.StartsWith("say;") || line.StartsWith("sayteam;"))
                                {
                                    log.LogDebug($"[{selectedLogFileMonitor.ServerTitle}] {line}");

                                    try
                                    {
                                        var parts = line.Split(';');
                                        var guid = parts[1];
                                        var name = parts[3];
                                        var message = parts[4].Trim();

                                        var chatCommandHandlers = _serviceProvider.GetServices<IChatCommand>();

                                        foreach (var chatCommandHandler in chatCommandHandlers)
                                            if (message.ToLower().StartsWith(chatCommandHandler.CommandText))
                                            {
                                                log.LogDebug($"ChatCommand handler {nameof(chatCommandHandler)} matched command");
                                                await chatCommandHandler.ProcessMessage(selectedLogFileMonitor.ServerId, name, guid, message);
                                            }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.LogWarning(ex, $"Failed to execute chat command for {selectedLogFileMonitor.ServerTitle} with data {line}");
                                        log.LogWarning(ex.Message);

                                        if (ex.InnerException != null)
                                            log.LogWarning(ex.InnerException.Message);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.LogWarning(ex, $"Failed to process chat message for {selectedLogFileMonitor.ServerTitle} with data {line}");
                                log.LogWarning(ex.Message);

                                if (ex.InnerException != null)
                                    log.LogWarning(ex.InnerException.Message);
                            }

                            byteList = new List<byte>();
                        }

                        prev = cur;
                    }

                    response?.Dispose();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Failed to read log file for {selectedLogFileMonitor.ServerTitle} against file {requestPath}");
                    log.LogError(ex.Message);

                    if (ex.InnerException != null)
                        log.LogError(ex.InnerException.Message);
                }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunMonitorLogFile @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
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