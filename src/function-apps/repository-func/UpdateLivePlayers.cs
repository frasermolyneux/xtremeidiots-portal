using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc
{
    public class UpdateLivePlayers
    {
        private readonly ILogger<UpdateLivePlayers> logger;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

        public UpdateLivePlayers(
            ILogger<UpdateLivePlayers> logger,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.serversApiClient = serversApiClient;
        }


        [FunctionName("UpdateLivePlayers")]
        public async Task RunUpdateLivePlayers([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            GameType[] gameTypes = new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5, GameType.Insurgency };
            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, null, 0, 0, null);

            foreach (var gameServerDto in gameServerDtos)
            {
                if (string.IsNullOrWhiteSpace(gameServerDto.Hostname) || gameServerDto.QueryPort == 0)
                    continue;

                if (!string.IsNullOrWhiteSpace(gameServerDto.RconPassword))
                {
                    await UpdateLivePlayersFromRcon(gameServerDto);
                }
                else
                {
                    await UpdateLivePlayersFromQuery(gameServerDto);
                }
            }
        }

        private async Task UpdateLivePlayersFromRcon(GameServerDto gameServerDto)
        {
            ServerRconStatusResponseDto serverRconStatusResponseDto = null;
            try
            {
                serverRconStatusResponseDto = await serversApiClient.Rcon.GetServerStatus(gameServerDto.Id);

                if (serverRconStatusResponseDto == null)
                    throw new Exception("Server rcon response was null");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"UpdateLivePlayers: Failed to get rcon server status for {gameServerDto.Title} - {gameServerDto.Hostname}:{gameServerDto.QueryPort}");
            }

            var livePlayerDtos = serverRconStatusResponseDto.Players.Select(rconPlayer => new LivePlayerDto
            {
                Name = rconPlayer.Name,
                IpAddress = rconPlayer.IpAddress,
                GameType = gameServerDto.GameType,
                GameServerServerId = gameServerDto.Id
            }).ToList();

            await repositoryApiClient.LivePlayers.CreateGameServerLivePlayers(gameServerDto.Id, livePlayerDtos);
        }

        private async Task UpdateLivePlayersFromQuery(GameServerDto gameServerDto)
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
            }

            var livePlayerDtos = serverQueryStatusResponseDto.Players.Select(queryPlayer => new LivePlayerDto
            {
                Name = queryPlayer.Name,
                Score = queryPlayer.Score,
                GameType = gameServerDto.GameType,
                GameServerServerId = gameServerDto.Id
            }).ToList();

            await repositoryApiClient.LivePlayers.CreateGameServerLivePlayers(gameServerDto.Id, livePlayerDtos);
        }
    }
}
