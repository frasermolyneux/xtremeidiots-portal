using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Players.Ingest
{
    public class PlayerIngest : IPlayerIngest
    {
        private readonly IPlayersCacheRepository _playersCacheRepository;
        private readonly IPlayersRepository _playersRepository;
        private ILogger _logger;

        public PlayerIngest(
            ILogger<PlayerIngest> logger,
            IPlayersCacheRepository playersCacheRepository,
            IPlayersRepository playersRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playersCacheRepository = playersCacheRepository ?? throw new ArgumentNullException(nameof(playersCacheRepository));
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
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

            var cachedPlayer = await _playersCacheRepository.GetPlayer(gameType, guid);

            if (cachedPlayer != null)
            {
                _logger.LogDebug("Player {Username} with Guid {Guid} for {GameType} exists in cache", cachedPlayer.Username, cachedPlayer.Guid, cachedPlayer.GameType);

                var playerDto = new PlayerDto
                {
                    PlayerId = cachedPlayer.PlayerId,
                    GameType = cachedPlayer.GameType,
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
                var player = await _playersRepository.GetPlayer(gameType, guid);

                if (player == null)
                {
                    _logger.LogInformation("Creating new player in the database with username {Username} guid {Guid}", username, guid);

                    var playerDto = new PlayerDto
                    {
                        GameType = gameType,
                        Username = username,
                        Guid = guid,
                        FirstSeen = DateTime.UtcNow,
                        LastSeen = DateTime.UtcNow
                    };

                    if (IPAddress.TryParse(ipAddress, out var address))
                        playerDto.IpAddress = ipAddress;

                    await _playersRepository.CreatePlayer(playerDto);
                }
                else
                {
                    _logger.LogDebug("Adding the player to the cache with guid {Guid}", guid);

                    var playerCacheEntity = new PlayerCacheEntity
                    {
                        PartitionKey = player.GameType.ToString(),
                        RowKey = player.Guid,
                        PlayerId = player.PlayerId,
                        GameType = player.GameType,
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