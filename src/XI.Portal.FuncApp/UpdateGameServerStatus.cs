using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XI.Portal.FuncApp
{
    // ReSharper disable once UnusedMember.Global
    public class UpdateGameServerStatus
    {
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IGameServerStatusStatsRepository _gameServerStatusStatsRepository;
        private readonly IPlayerIngest _playerIngest;
        private readonly IRepositoryApiClient repositoryApiClient;

        public UpdateGameServerStatus(
            IGameServerStatusRepository gameServerStatusRepository,
            IGameServerStatusStatsRepository gameServerStatusStatsRepository,
            IPlayerIngest playerIngest,
            IRepositoryApiClient repositoryApiClient)
        {
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _gameServerStatusStatsRepository = gameServerStatusStatsRepository ?? throw new ArgumentNullException(nameof(gameServerStatusStatsRepository));
            _playerIngest = playerIngest ?? throw new ArgumentNullException(nameof(playerIngest));

            this.repositoryApiClient = repositoryApiClient;
        }

        [FunctionName("UpdateGameServerStatus")]
        // ReSharper disable once UnusedMember.Global
        public async Task RunUpdateGameServerStatus([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogDebug($"Start RunUpdateGameServerStatus @ {DateTime.UtcNow}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _playerIngest.OverrideLogger(log);

            var servers = await repositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.ShowOnPortalServerList, 0, 0, null);

            foreach (var server in servers)
            {
                log.LogDebug($"Updating game server status for {server.Title}");

                try
                {
                    var model = await _gameServerStatusRepository.GetStatus(server.Id, TimeSpan.FromMinutes(-1));

                    if (model == null)
                    {
                        log.LogWarning($"Failed to update game server status for {server.Title}");
                        continue;
                    }

                    log.LogInformation($"{model.ServerName} is online running {model.Map} with {model.PlayerCount} players connected");

                    await _gameServerStatusStatsRepository.UpdateEntry(new GameServerStatusStatsDto
                    {
                        ServerId = server.Id,
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

            foreach (var gameServerStatus in await _gameServerStatusRepository.GetAllStatusModels(new GameServerStatusFilterModel(), TimeSpan.Zero))
            {
                var server = servers.SingleOrDefault(s => s.Id == gameServerStatus.ServerId);

                if (server == null)
                {
                    log.LogInformation($"Removing game server status as server is no longer being queried {gameServerStatus.ServerName}");
                    await _gameServerStatusRepository.DeleteStatusModel(gameServerStatus);
                    await _gameServerStatusStatsRepository.DeleteGameServerStatusStats(gameServerStatus.ServerId);
                }
            }

            stopWatch.Stop();
            log.LogDebug($"Stop RunUpdateGameServerStatus @ {DateTime.UtcNow} after {stopWatch.ElapsedMilliseconds} milliseconds");
        }
    }
}