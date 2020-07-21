using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.Portal.Data.Legacy;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class UpdateGameServerStatus
    {
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IGameServerStatusStatsRepository _gameServerStatusStatsRepository;
        private readonly LegacyPortalContext _legacyContext;
        private readonly IPlayerIngest _playerIngest;

        public UpdateGameServerStatus(
            LegacyPortalContext legacyContext,
            IGameServerStatusRepository gameServerStatusRepository,
            IGameServerStatusStatsRepository gameServerStatusStatsRepository,
            IPlayerIngest playerIngest)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _gameServerStatusStatsRepository = gameServerStatusStatsRepository ?? throw new ArgumentNullException(nameof(gameServerStatusStatsRepository));
            _playerIngest = playerIngest ?? throw new ArgumentNullException(nameof(playerIngest));
        }

        [FunctionName("UpdateGameServerStatus")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunUpdateGameServerStatus([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunUpdateGameServerStatus @ {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _playerIngest.OverrideLogger(log);

            var servers = await _legacyContext.GameServers.Where(server => server.ShowOnPortalServerList).ToListAsync();

            foreach (var server in servers)
            {
                log.LogDebug($"Updating game server status for {server.Title}");

                try
                {
                    var model = await _gameServerStatusRepository.GetStatus(server.ServerId,TimeSpan.FromMinutes(-1));

                    if (model == null)
                    {
                        log.LogWarning($"Failed to update game server status for {server.Title}");
                        continue;
                    }

                    log.LogInformation($"{model.ServerName} is online running {model.Map} with {model.PlayerCount} players connected");

                    await _gameServerStatusStatsRepository.UpdateEntry(new GameServerStatusStatsDto
                    {
                        ServerId = server.ServerId,
                        GameType = server.GameType,
                        PlayerCount = model.PlayerCount,
                        MapName = model.Map
                    });

                    var playerGuid = string.Empty;
                    try
                    {
                        foreach (var player in model.Players)
                        {
                            playerGuid = player.Guid;
                            await _playerIngest.IngestData(server.GameType, player.Guid, player.Name, player.IpAddress);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Failed to ingest player data for guid: {Guid}", playerGuid);
                    }

                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failed to get game server status for {Title}", server.Title);
                }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunUpdateGameServerStatus @ {DateTime.Now} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}