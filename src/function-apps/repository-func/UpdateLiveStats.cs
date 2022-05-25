using FM.GeoLocation.Contract.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
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

        public UpdateLiveStats(
            ILogger<UpdateLiveStats> logger,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient,
            IGeoLocationClient geoLocationClient,
            TelemetryClient telemetryClient)
        {
            this.logger = logger;
            this.repositoryApiClient = repositoryApiClient;
            this.serversApiClient = serversApiClient;
            this.geoLocationClient = geoLocationClient;
            this.telemetryClient = telemetryClient;
        }


        [FunctionName("UpdateLiveStats")]
        public async Task RunUpdateLiveStats([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            GameType[] gameTypes = new GameType[] { GameType.CallOfDuty2, GameType.CallOfDuty4, GameType.CallOfDuty5, GameType.Insurgency };
            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, null, GameServerFilter.LiveStatusEnabled, 0, 0, null);

            foreach (var gameServerDto in gameServerDtos)
            {
                if (string.IsNullOrWhiteSpace(gameServerDto.Hostname) || gameServerDto.QueryPort == 0)
                    continue;

                var livePlayerDtos = new List<LivePlayerDto>();

                if (!string.IsNullOrWhiteSpace(gameServerDto.RconPassword))
                {
                    livePlayerDtos = await UpdateLivePlayersFromRcon(gameServerDto);
                    livePlayerDtos = await UpdateLivePlayersFromQuery(gameServerDto, livePlayerDtos);
                    livePlayerDtos = await EnrichPlayersWithGeoLocation(livePlayerDtos);
                }
                else
                {
                    livePlayerDtos = await UpdateLivePlayersFromQuery(gameServerDto, livePlayerDtos);
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
                return new List<LivePlayerDto>();
            }

            var livePlayerDtos = serverRconStatusResponseDto.Players.Select(rconPlayer => new LivePlayerDto
            {
                Name = rconPlayer.Name,
                Ping = rconPlayer.Ping,
                Num = rconPlayer.Num,
                Rate = rconPlayer.Rate,
                IpAddress = rconPlayer.IpAddress,
                GameType = gameServerDto.GameType,
                GameServerServerId = gameServerDto.Id
            }).ToList();

            return livePlayerDtos;
        }

        private async Task<List<LivePlayerDto>> UpdateLivePlayersFromQuery(GameServerDto gameServerDto, List<LivePlayerDto> livePlayerDtos)
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
                return livePlayerDtos;
            }

            foreach (var queryPlayer in serverQueryStatusResponseDto.Players)
            {
                var livePlayer = livePlayerDtos.SingleOrDefault(lp => lp.Name.NormalizeName() == queryPlayer.Name.NormalizeName());

                if (livePlayer != null)
                {
                    livePlayer.Score = queryPlayer.Score;
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
    }
}
