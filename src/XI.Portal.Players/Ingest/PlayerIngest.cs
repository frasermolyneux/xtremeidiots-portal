using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Players.Ingest
{
    public class PlayerIngest : IPlayerIngest
    {
        private readonly IPlayersCacheRepository _playersCacheRepository;
        private ILogger _logger;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;

        public PlayerIngest(
            ILogger<PlayerIngest> logger,
            IPlayersCacheRepository playersCacheRepository,
            IRepositoryApiClient repositoryApiClient,
            IRepositoryTokenProvider repositoryTokenProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playersCacheRepository = playersCacheRepository ?? throw new ArgumentNullException(nameof(playersCacheRepository));
            this.repositoryApiClient = repositoryApiClient;
            this.repositoryTokenProvider = repositoryTokenProvider;
        }

        public async Task IngestData(GameType gameType, string guid, string username, string ipAddress)
        {
            if (gameType == GameType.Unknown || string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Ingest ignored as it was invalid with gameType '{GameType}', guid '{Guid}', username '{Username}'", gameType, guid, username);
                return;
            }

            if (username == "allies" || username == "axis")
            {
                _logger.LogWarning("Ingest data ignored as username was {Username}", username);
                return;
            }

            _logger.LogDebug("Ingesting gameType '{GameType}', guid '{Guid}', username '{Username}', ipAddress '{IpAddress}'", gameType, guid, username, ipAddress);

            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();

            var cachedPlayer = await _playersCacheRepository.GetPlayer(gameType, guid);

            if (cachedPlayer != null)
            {
                _logger.LogDebug("Player {Username} with Guid {Guid} for {GameType} exists in cache", cachedPlayer.Username, cachedPlayer.Guid, cachedPlayer.GameType);

                var playerDto = new PlayerDto
                {
                    Id = cachedPlayer.PlayerId,
                    GameType = cachedPlayer.GameType.ToString(),
                    Username = cachedPlayer.Username,
                    Guid = cachedPlayer.Guid,
                    IpAddress = cachedPlayer.IpAddress,
                    FirstSeen = cachedPlayer.FirstSeen,
                    LastSeen = cachedPlayer.LastSeen
                };

                var update = false;

                if (cachedPlayer.Username != username)
                {
                    playerDto.Username = username;
                    update = true;
                }

                if (IPAddress.TryParse(ipAddress, out var address))
                    if (cachedPlayer.IpAddress != ipAddress)
                    {
                        playerDto.IpAddress = ipAddress;
                        update = true;
                    }

                if (cachedPlayer.LastSeen < DateTime.UtcNow.AddMinutes(-10)) update = true;

                if (update)
                {
                    _logger.LogDebug("Updating database player information for {Guid}", guid);

                    cachedPlayer.Username = username;
                    cachedPlayer.IpAddress = ipAddress;
                    cachedPlayer.LastSeen = DateTime.UtcNow;

                    await _playersCacheRepository.UpdatePlayer(cachedPlayer);
                }
            }
            else
            {
                var player = await repositoryApiClient.PlayersApiClient.GetPlayerByGameType(accessToken, gameType.ToString(), guid);

                if (player == null)
                {
                    _logger.LogInformation("Creating new player in the database with username {Username} guid {Guid}", username, guid);

                    await repositoryApiClient.PlayersApiClient.CreatePlayer(accessToken, new XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models.PlayerDto()
                    {
                        GameType = gameType.ToString(),
                        Username = username,
                        Guid = guid,
                        IpAddress = ipAddress
                    });
                }
                else
                {
                    _logger.LogDebug("Adding the player to the cache with guid {Guid}", guid);

                    var playerCacheEntity = new PlayerCacheEntity
                    {
                        PartitionKey = player.GameType.ToString(),
                        RowKey = player.Guid,
                        PlayerId = player.Id,
                        GameType = Enum.Parse<GameType>(player.GameType),
                        Username = player.Username,
                        Guid = player.Guid,
                        IpAddress = player.IpAddress,
                        FirstSeen = player.FirstSeen,
                        LastSeen = player.LastSeen
                    };

                    await _playersCacheRepository.UpdatePlayer(playerCacheEntity);
                }
            }
        }

        public void OverrideLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}