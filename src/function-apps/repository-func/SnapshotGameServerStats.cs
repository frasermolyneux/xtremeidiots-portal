using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc
{
    public class SnapshotGameServerStats
    {
        private readonly ILogger<SnapshotGameServerStats> logger;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

        public SnapshotGameServerStats(
            ILogger<SnapshotGameServerStats> logger,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.serversApiClient = serversApiClient;
        }

        [FunctionName("SnapshotGameServerStats")]
        public async Task RunSnapshotGameServerStats([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            GameType[] gameTypes = new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5, GameType.Insurgency };
            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, GameServerFilter.LiveStatusEnabled, 0, 0, null);

            List<GameServerStatDto> gameServerStatDtos = new List<GameServerStatDto>();

            foreach (var gameServerDto in gameServerDtos)
            {
                if (string.IsNullOrWhiteSpace(gameServerDto.Hostname) || gameServerDto.QueryPort == 0)
                    continue;

                if (!string.IsNullOrWhiteSpace(gameServerDto.RconPassword))
                {
                    ServerQueryStatusResponseDto serverQueryStatusResponseDto = null;
                    try
                    {
                        serverQueryStatusResponseDto = await serversApiClient.Query.GetServerStatus(gameServerDto.Id);

                        if (serverQueryStatusResponseDto == null)
                            throw new Exception("Server query response was null");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"UpdateLivePlayers: Failed to get query server status for {gameServerDto.Title} - {gameServerDto.Hostname}:{gameServerDto.QueryPort}");
                        continue;
                    }

                    gameServerStatDtos.Add(new GameServerStatDto
                    {
                        GameServerId = gameServerDto.Id,
                        PlayerCount = serverQueryStatusResponseDto.PlayerCount,
                        MapName = serverQueryStatusResponseDto.Map
                    });
                }
            }

            if (gameServerStatDtos.Any())
                await repositoryApiClient.GameServersStats.CreateGameServerStats(gameServerStatDtos);
        }
    }
}
