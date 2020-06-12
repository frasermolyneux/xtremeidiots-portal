using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.Portal.Maps.Dto;
using XI.Portal.Maps.Interfaces;
using XI.Portal.Players.Interfaces;
using XI.Portal.Servers.Integrations.Interfaces;
using XI.Portal.Servers.Interfaces;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Integrations
{
    public class MapPopularityCommand : IChatCommand
    {
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IPlayersRepository _playersRepository;
        private readonly ILogger<MapPopularityCommand> _logger;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IMapPopularityRepository _mapPopularityRepository;
        private readonly IRconClientFactory _rconClientFactory;

        public MapPopularityCommand(
            ILogger<MapPopularityCommand> logger,
            IGameServersRepository gameServersRepository,
            IMapPopularityRepository mapPopularityRepository,
            IRconClientFactory rconClientFactory,
            IGameServerStatusRepository gameServerStatusRepository,
            IPlayersRepository playersRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _mapPopularityRepository = mapPopularityRepository ?? throw new ArgumentNullException(nameof(mapPopularityRepository));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
        }

        public string[] CommandAliases { get; } = {"!like", "!dislike"};

        public async Task ProcessMessage(Guid serverId, string name, string guid, string message)
        {
            var server = await _gameServersRepository.GetGameServer(serverId);

            var gameServerStatus = await _gameServerStatusRepository.GetStatus(serverId, TimeSpan.Zero);
            if (gameServerStatus == null)
            {
                _logger.LogWarning("Could not process !like for {name} as the game server status is null for {serverId}", name, serverId);
                return;
            }

            var statusPlayer = gameServerStatus.Players.SingleOrDefault(p => p.Guid == guid);
            if (statusPlayer == null)
            {
                _logger.LogWarning("Could not process !like for {name} as the status player is null for {guid}", name, guid);
                return;
            }

            var databasePlayer = await _playersRepository.GetPlayer(gameServerStatus.GameType, guid);
            if (databasePlayer == null)
            {
                _logger.LogWarning("Could not process !like for {name} as player is null for {guid}", name, guid);
                return;
            }

            var rconClient = _rconClientFactory.CreateInstance(server.GameType, server.ServerId, server.Hostname, server.QueryPort, server.RconPassword);
            var like = !message.ToLower().Contains("!dislike");

            var mapPopularityDto = await _mapPopularityRepository.GetMapPopularity(gameServerStatus.GameType, gameServerStatus.Map);

            if (mapPopularityDto == null)
            {
                mapPopularityDto = new MapPopularityDto
                {
                    GameType = gameServerStatus.GameType,
                    MapName = gameServerStatus.Map,
                    MapVotes = new List<MapPopularityVoteDto>()
                    {
                        new MapPopularityVoteDto
                        {
                            ServerId = gameServerStatus.ServerId,
                            ServerName = gameServerStatus.ServerName,
                            PlayerId = databasePlayer.PlayerId,
                            PlayerName = name,
                            ModName = gameServerStatus.Mod,
                            PlayerCount = gameServerStatus.PlayerCount,
                            Updated = DateTime.UtcNow,
                            Like = like
                        }
                    }
                };

                await _mapPopularityRepository.UpdateMapPopularity(mapPopularityDto);
            }
            else
            {
                var existing = mapPopularityDto.MapVotes.SingleOrDefault(mp => mp.PlayerId == databasePlayer.PlayerId && mp.ModName == gameServerStatus.Mod && mp.ServerId == gameServerStatus.ServerId);
                if (existing == null)
                {
                    mapPopularityDto.MapVotes.Add(new MapPopularityVoteDto
                    {
                        ServerId = gameServerStatus.ServerId,
                        ServerName = gameServerStatus.ServerName,
                        PlayerId = databasePlayer.PlayerId,
                        PlayerName = name,
                        ModName = gameServerStatus.Mod,
                        PlayerCount = gameServerStatus.PlayerCount,
                        Updated = DateTime.UtcNow,
                        Like = like
                    });

                    await _mapPopularityRepository.UpdateMapPopularity(mapPopularityDto);
                }
                else
                {
                    existing.Updated = DateTime.UtcNow;
                    existing.Like = like;

                    await _mapPopularityRepository.UpdateMapPopularity(mapPopularityDto);
                }
            }

            var globalMessage = $"^6{name} ^2likes ^6this map - thanks for the feedback!";
            if (!like)
                globalMessage = $"^6{name} ^1dislikes ^6this map - thanks for the feedback!";

            var totalLikes = mapPopularityDto.MapVotes.Count(mv => mv.ServerId == gameServerStatus.ServerId && mv.ModName == gameServerStatus.Mod && mv.Like == true);
            var totalDislikes = mapPopularityDto.MapVotes.Count(mv => mv.ServerId == gameServerStatus.ServerId && mv.ModName == gameServerStatus.Mod && mv.Like == false);

            var overall = $"^6Overall there are ^2{totalLikes} likes ^6and ^1{totalDislikes} dislikes ^6for this map";

            await rconClient.Say(globalMessage);
            await rconClient.Say(overall);
        }
    }
}