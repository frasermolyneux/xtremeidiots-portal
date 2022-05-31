using FM.GeoLocation.Contract.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.RepositoryFunc.Extensions;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.RepositoryFunc
{
    public class UpdateLiveStats
    {
        private readonly ILogger<UpdateLiveStats> logger;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;
        private readonly IGeoLocationClient geoLocationClient;
        private readonly TelemetryClient telemetryClient;
        private readonly IMemoryCache memoryCache;

        public UpdateLiveStats(
            ILogger<UpdateLiveStats> logger,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient,
            IGeoLocationClient geoLocationClient,
            TelemetryClient telemetryClient,
            IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.serversApiClient = serversApiClient;
            this.geoLocationClient = geoLocationClient;
            this.telemetryClient = telemetryClient;
            this.memoryCache = memoryCache;
        }


        [FunctionName("UpdateLiveStats")]
        public async Task RunUpdateLiveStats([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            var gameTypes = new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5, GameType.Insurgency };
            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, GameServerFilter.LiveStatusEnabled, 0, 0, null);

            if (gameServerDtos == null)
            {
                logger.LogCritical("Failed to retrieve game servers from repository");
                return;
            }

            foreach (var gameServerDto in gameServerDtos)
            {
                if (string.IsNullOrWhiteSpace(gameServerDto.Hostname) || gameServerDto.QueryPort == 0)
                    continue;

                var livePlayerDtos = new List<LivePlayerDto>();

                try
                {
                    if (!string.IsNullOrWhiteSpace(gameServerDto.RconPassword))
                    {
                        livePlayerDtos = await UpdateLivePlayersFromRcon(gameServerDto);
                        livePlayerDtos = await UpdateLivePlayersFromQuery(gameServerDto, livePlayerDtos);
                        livePlayerDtos = await EnrichPlayersWithGeoLocation(livePlayerDtos);

                        await UpdateRecentPlayersWithLivePlayers(livePlayerDtos);
                    }
                    else
                    {
                        livePlayerDtos = await UpdateLivePlayersFromQuery(gameServerDto, livePlayerDtos);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to update live stats for server '{gameServerDto.Id}'");
                    continue;
                }

                MetricTelemetry telemetry = new()
                {
                    Name = "PlayerCount",
                    Sum = livePlayerDtos.Count
                };

                telemetry.Properties.Add("serverId", gameServerDto.Id.ToString());
                telemetry.Properties.Add("serverName", gameServerDto.Title);

                telemetryClient.TrackMetric(telemetry);

                await repositoryApiClient.LivePlayers.CreateGameServerLivePlayers(gameServerDto.Id, livePlayerDtos);
            }
        }

        private async Task<List<LivePlayerDto>> UpdateLivePlayersFromRcon(GameServerDto gameServerDto)
        {
            ServerRconStatusResponseDto? serverRconStatusResponseDto = null;
            try
            {
                serverRconStatusResponseDto = await serversApiClient.Rcon.GetServerStatus(gameServerDto.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get rcon server status for '{gameServerDto.Id}'");
                return new List<LivePlayerDto>();
            }

            if (serverRconStatusResponseDto == null)
                throw new NullReferenceException($"Server rcon status was null for '{gameServerDto.Id}'");

            var livePlayerDtos = new List<LivePlayerDto>();
            foreach (var rconPlayer in serverRconStatusResponseDto.Players)
            {
                var livePlayerDto = new LivePlayerDto
                {
                    Name = rconPlayer.Name,
                    Ping = rconPlayer.Ping,
                    Num = rconPlayer.Num,
                    Rate = rconPlayer.Rate,
                    IpAddress = rconPlayer.IpAddress,
                    GameType = gameServerDto.GameType,
                    GameServerServerId = gameServerDto.Id
                };

                if (!string.IsNullOrWhiteSpace(rconPlayer.Guid))
                {
                    var playerId = await GetPlayerId(gameServerDto.GameType, rconPlayer.Guid);
                    if (playerId.HasValue)
                    {
                        livePlayerDto.PlayerId = playerId;
                    }
                }

                livePlayerDtos.Add(livePlayerDto);
            }

            return livePlayerDtos;
        }

        private async Task<List<LivePlayerDto>> UpdateLivePlayersFromQuery(GameServerDto gameServerDto, List<LivePlayerDto> livePlayerDtos)
        {
            ServerQueryStatusResponseDto? serverQueryStatusResponseDto = null;
            try
            {
                serverQueryStatusResponseDto = await serversApiClient.Query.GetServerStatus(gameServerDto.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get query server status for '{gameServerDto.Id}'");
                return livePlayerDtos;
            }

            if (serverQueryStatusResponseDto == null)
                throw new NullReferenceException($"Server query status was null for '{gameServerDto.Id}'");

            foreach (var livePlayerDto in livePlayerDtos)
            {
                var queryPlayer = serverQueryStatusResponseDto.Players.SingleOrDefault(qp => qp.Name?.NormalizeName() == livePlayerDto.Name?.NormalizeName());

                if (queryPlayer != null)
                {
                    livePlayerDto.Score = queryPlayer.Score;
                }
            }

            gameServerDto.LiveTitle = serverQueryStatusResponseDto.ServerName;
            gameServerDto.LiveMap = serverQueryStatusResponseDto.Map;
            gameServerDto.LiveMod = serverQueryStatusResponseDto.Mod;
            gameServerDto.LiveMaxPlayers = serverQueryStatusResponseDto.MaxPlayers;
            gameServerDto.LiveCurrentPlayers = serverQueryStatusResponseDto.PlayerCount;
            gameServerDto.LiveLastUpdated = DateTime.UtcNow;

            await repositoryApiClient.GameServers.UpdateGameServer(gameServerDto);

            return livePlayerDtos;
        }

        private async Task<List<LivePlayerDto>> EnrichPlayersWithGeoLocation(List<LivePlayerDto> livePlayerDtos)
        {
            foreach (var livePlayerDto in livePlayerDtos)
            {
                if (string.IsNullOrWhiteSpace(livePlayerDto.IpAddress))
                    continue;

                var lookupAddressResponse = await geoLocationClient.LookupAddress(livePlayerDto.IpAddress);

                if (lookupAddressResponse.Success)
                {
                    livePlayerDto.Lat = lookupAddressResponse.GeoLocationDto.Latitude;
                    livePlayerDto.Long = lookupAddressResponse.GeoLocationDto.Longitude;
                    livePlayerDto.CountryCode = lookupAddressResponse.GeoLocationDto.CountryCode;
                }
            }

            return livePlayerDtos;
        }

        private async Task UpdateRecentPlayersWithLivePlayers(List<LivePlayerDto> livePlayerDtos)
        {
            var createRecentPlayerDtos = new List<CreateRecentPlayerDto>();

            foreach (var livePlayer in livePlayerDtos)
            {
                if (string.IsNullOrWhiteSpace(livePlayer.Name) || !livePlayer.PlayerId.HasValue)
                    continue;

                var createRecentPlayerDto = new CreateRecentPlayerDto(livePlayer.Name, livePlayer.GameType, (Guid)livePlayer.PlayerId)
                {
                    IpAddress = livePlayer.IpAddress,
                    Lat = livePlayer.Lat,
                    Long = livePlayer.Long,
                    CountryCode = livePlayer.CountryCode,
                    ServerId = livePlayer.GameServerServerId
                };

                createRecentPlayerDtos.Add(createRecentPlayerDto);
            }

            if (createRecentPlayerDtos.Any())
                await repositoryApiClient.RecentPlayers.CreateRecentPlayers(createRecentPlayerDtos);
        }

        private async Task<Guid?> GetPlayerId(GameType gameType, string guid)
        {
            var cacheKey = $"{gameType}-${guid}";

            if (memoryCache.TryGetValue(cacheKey, out Guid playerId))
                return playerId;

            var player = await repositoryApiClient.Players.GetPlayerByGameType(gameType, guid);

            if (player != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15));
                memoryCache.Set(cacheKey, player.Id, cacheEntryOptions);

                return player.Id;
            }

            return null;
        }
    }
}
