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

                    log.LogInformation($"Creating new log file monitor state object for {fileMonitorDto.GameServer.Title} against path {fileMonitorDto.FilePath}");
                }
                else
                {
                    fileMonitorState.ServerTitle = fileMonitorDto.GameServer.Title;

                    if (fileMonitorState.FilePath != fileMonitorDto.FilePath)
                    {
                        fileMonitorState.FilePath = fileMonitorDto.FilePath;
                        fileMonitorState.RemoteSize = -1;
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

        [FunctionName("MonitorLogFile")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunMonitorLogFile([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunMonitorLogFile @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                var fileMonitorStates = await _logFileMonitorStateRepository.GetLogFileMonitorStates();
                var gameServerStatus = await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero);

                log.LogDebug($"Processing {fileMonitorStates.Count} file monitor states");

                foreach (var fileMonitorStateDto in fileMonitorStates)
                {
                    var requestPath = $"ftp://{fileMonitorStateDto.FtpHostname}{fileMonitorStateDto.FilePath}";
                    try

                    {
                        var statusModel = gameServerStatus.SingleOrDefault(s => s.ServerId == fileMonitorStateDto.ServerId);

                        if (statusModel == null)
                        {
                            log.LogWarning($"There is no game server status model for {fileMonitorStateDto.ServerTitle}");
                            continue;
                        }

                        if (statusModel.PlayerCount > 0)
                        {
                            log.LogDebug($"Performing request for {fileMonitorStateDto.ServerTitle} against file {requestPath} as player count is {statusModel.PlayerCount}");

                            if (fileMonitorStateDto.RemoteSize == -1 || fileMonitorStateDto.LastRead < DateTime.UtcNow.AddMinutes(-1))
                            {
                                log.LogDebug($"The remote file for {fileMonitorStateDto.ServerTitle} ({requestPath}) has not been read in the past minute");

                                var fileSize = GetFileSize(fileMonitorStateDto.FtpUsername, fileMonitorStateDto.FtpPassword, requestPath);
                                log.LogDebug($"The remote file size for {fileMonitorStateDto.ServerTitle} is {fileSize} bytes");

                                fileMonitorStateDto.LastRead = DateTime.UtcNow;
                                fileMonitorStateDto.RemoteSize = fileSize;

                                await _logFileMonitorStateRepository.UpdateState(fileMonitorStateDto);
                            }
                            else
                            {
                                try
                                {
                                    var request = (FtpWebRequest) WebRequest.Create(requestPath);
                                    request.Credentials = new NetworkCredential(fileMonitorStateDto.FtpUsername, fileMonitorStateDto.FtpPassword);
                                    request.ContentOffset = fileMonitorStateDto.RemoteSize;
                                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                                    using (var stream = request.GetResponse().GetResponseStream())
                                    {
                                        using (var sr = new StreamReader(stream ?? throw new InvalidOperationException()))
                                        {
                                            var prev = -1;

                                            var byteList = new List<byte>();

                                            while (true)
                                            {
                                                var cur = sr.Read();

                                                if (cur == -1) break;

                                                byteList.Add((byte) cur);

                                                if (prev == '\r' && cur == '\n')
                                                {
                                                    fileMonitorStateDto.RemoteSize += byteList.Count;
                                                    fileMonitorStateDto.LastRead = DateTime.UtcNow;

                                                    await _logFileMonitorStateRepository.UpdateState(fileMonitorStateDto);

                                                    var line = Encoding.UTF8.GetString(byteList.ToArray()).TrimEnd('\n');
                                                    try
                                                    {
                                                        line = line.Replace("\r\n", "");
                                                        line = line.Trim();
                                                        line = line.Substring(line.IndexOf(' ') + 1);

                                                        if (line.StartsWith("say;") || line.StartsWith("sayteam;"))
                                                        {
                                                            log.LogDebug($"[{fileMonitorStateDto.ServerTitle}] {line}");

                                                            try
                                                            {
                                                                var parts = line.Split(';');
                                                                var guid = parts[1];
                                                                var name = parts[3];
                                                                var message = parts[4];

                                                                var chatCommandHandlers = _serviceProvider.GetServices<IChatCommand>();

                                                                foreach (var chatCommandHandler in chatCommandHandlers)
                                                                {
                                                                    if (message.ToLower().StartsWith(chatCommandHandler.CommandText))
                                                                    {
                                                                        log.LogDebug($"ChatCommand handler {nameof(chatCommandHandler)} matched command");
                                                                        await chatCommandHandler.ProcessMessage(fileMonitorStateDto.ServerId, name, guid, message);
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                log.LogWarning(ex, $"Failed to execute chat command for {fileMonitorStateDto.ServerTitle} with data {line}");
                                                                log.LogWarning(ex.Message);

                                                                if (ex.InnerException != null)
                                                                    log.LogWarning(ex.InnerException.Message);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        log.LogWarning(ex, $"Failed to process chat message for {fileMonitorStateDto.ServerTitle} with data {line}");
                                                        log.LogWarning(ex.Message);

                                                        if (ex.InnerException != null)
                                                            log.LogWarning(ex.InnerException.Message);
                                                    }

                                                    byteList = new List<byte>();
                                                }

                                                prev = cur;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.LogError(ex, $"Failed to read log file for {fileMonitorStateDto.ServerTitle} against file {requestPath}");
                                    log.LogError(ex.Message);

                                    if (ex.InnerException != null)
                                        log.LogError(ex.InnerException.Message);
                                }
                            }
                        }
                        else
                        {
                            log.LogDebug($"Skipping monitor as the player count is 0 for {fileMonitorStateDto.ServerTitle}");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"Exception processing request for {fileMonitorStateDto.ServerTitle} against file {requestPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Top-level exception processing the MonitorLogFile function");
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunMonitorLogFile @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }

        private static long GetFileSize(string username, string password, string requestPath)
        {
            var request = (FtpWebRequest) WebRequest.Create(requestPath);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            var fileSize = ((FtpWebResponse) request.GetResponse()).ContentLength;
            return fileSize;
        }
    }
}