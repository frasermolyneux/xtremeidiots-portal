using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
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
        private readonly IMemoryCache memoryCache;

        public SnapshotGameServerStats(
            ILogger<SnapshotGameServerStats> logger,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient,
            IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.serversApiClient = serversApiClient;
            this.memoryCache = memoryCache;
        }

        [FunctionName("SnapshotGameServerStats")]
        public async Task RunSnapshotGameServerStats([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            GameType[] gameTypes = new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5, GameType.Insurgency };
            var gameServersApiResponse = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, GameServerFilter.LiveTrackingEnabled, 0, 50, null);

            List<CreateGameServerStatDto> gameServerStatDtos = new List<CreateGameServerStatDto>();

            foreach (var gameServerDto in gameServersApiResponse.Result.Entries)
            {
                if (string.IsNullOrWhiteSpace(gameServerDto.Hostname) || gameServerDto.QueryPort == 0)
                    continue;

                if (!string.IsNullOrWhiteSpace(gameServerDto.RconPassword))
                {
                    ServerQueryStatusResponseDto serverQueryStatusResponseDto = null;
                    try
                    {
                        serverQueryStatusResponseDto = await serversApiClient.Query.GetServerStatus(gameServerDto.GameServerId);

                        if (serverQueryStatusResponseDto == null)
                            throw new Exception("Server query response was null");

                        if (!string.IsNullOrWhiteSpace(serverQueryStatusResponseDto.Map))
                        {
                            if (!memoryCache.TryGetValue($"{gameServerDto.GameType}-{serverQueryStatusResponseDto.Map}", out bool mapExists))
                            {
                                var mapDto = await repositoryApiClient.Maps.GetMap(gameServerDto.GameType, serverQueryStatusResponseDto.Map);

                                if (mapDto == null)
                                    await repositoryApiClient.Maps.CreateMap(new CreateMapDto(gameServerDto.GameType, serverQueryStatusResponseDto.Map));

                                memoryCache.Set($"{gameServerDto.GameType}-{serverQueryStatusResponseDto.Map}", true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"UpdateLivePlayers: Failed to get query server status for {gameServerDto.Title} - {gameServerDto.Hostname}:{gameServerDto.QueryPort}");
                        continue;
                    }

                    gameServerStatDtos.Add(new CreateGameServerStatDto(gameServerDto.GameServerId, serverQueryStatusResponseDto.PlayerCount, serverQueryStatusResponseDto.Map));
                }
            }

            if (gameServerStatDtos.Any())
                await repositoryApiClient.GameServersStats.CreateGameServerStats(gameServerStatDtos);
        }
    }
}
