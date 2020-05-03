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
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _playerIngest.OverrideLogger(log);

            var servers = await _legacyContext.GameServers.Where(server => server.ShowOnPortalServerList).ToListAsync();

            foreach (var server in servers)
            {
                log.LogInformation("Updating game server status for {Title}", server.Title);

                try
                {
                    var model = await _gameServerStatusRepository.GetStatus(server.ServerId, null, null, TimeSpan.FromMinutes(-5));
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
            log.LogInformation($"C# Timer completed after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}