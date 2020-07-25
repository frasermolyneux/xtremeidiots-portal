using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Servers.Dto;
using XI.Servers.Interfaces;
using XI.Servers.Interfaces.Models;
using XI.Servers.Models;

namespace XI.Servers.Clients
{
    internal class GameServerClient : IGameServerClient
    {
        private readonly ILogger _logger;
        private readonly IQueryClientFactory _queryClientFactory;
        private readonly IRconClientFactory _rconClientFactory;

        private GameType _gameType;

        private string _hostname;

        private DateTime _lastRconUpdate = DateTime.UtcNow;
        private int _queryPort;
        private Guid _serverIdentifier;

        public GameServerClient(ILogger logger, IQueryClientFactory queryClientFactory, IRconClientFactory rconClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryClientFactory = queryClientFactory ?? throw new ArgumentNullException(nameof(queryClientFactory));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        private IQueryClient QueryClient { get; set; }
        private IRconClient RconClient { get; set; }

        public List<IGameServerPlayer> Players { get; set; } = new List<IGameServerPlayer>();

        public void Configure(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword)
        {
            QueryClient = _queryClientFactory.CreateInstance(gameType, hostname, queryPort);

            if (!string.IsNullOrWhiteSpace(rconPassword))
                RconClient = _rconClientFactory.CreateInstance(gameType, serverId, hostname, queryPort, rconPassword);

            _gameType = gameType;
            _serverIdentifier = serverId;
            _hostname = hostname;
            _queryPort = queryPort;
        }

        public async Task<GameServerStatusDto> GetServerStatus()
        {
            var queryResponse = await QueryClient.GetServerStatus();

            var needsQuerySync = false;
            var needsRconSync = false;

            foreach (var queryPlayer in queryResponse.Players)
            {
                var existingPlayer = (GameServerPlayer) Players.SingleOrDefault(player => player.NormalizedName == queryPlayer.NormalizedName);

                if (existingPlayer == null)
                {
                    needsQuerySync = true;
                    needsRconSync = true;
                }
                else
                {
                    if (existingPlayer.RconPlayer == null)
                        needsRconSync = true;
                }
            }

            if (DateTime.UtcNow.AddSeconds(-30) > _lastRconUpdate)
            {
                needsQuerySync = true;
                needsRconSync = true;
            }

            if (needsQuerySync) RefreshWithGameServerStatus(queryResponse);

            if (needsRconSync && RconClient != null)
            {
                var rconPlayers = RconClient.GetPlayers();
                RefreshWithRconPlayers(rconPlayers);
                _lastRconUpdate = DateTime.UtcNow;
            }

            return new GameServerStatusDto
            {
                ServerId = _serverIdentifier,
                GameType = _gameType,
                Hostname = _hostname,
                QueryPort = _queryPort,
                ServerName = queryResponse.ServerName,
                Map = queryResponse.Map,
                Mod = queryResponse.Mod,
                PlayerCount = queryResponse.PlayerCount,
                MaxPlayers = queryResponse.MaxPlayers,
                Players = Players.Select(player => new GameServerPlayerDto
                {
                    Num = player.Num,
                    Guid = player.Guid,
                    Name = player.Name,
                    IpAddress = player.IpAddress,
                    Score = player.Score,
                    Rate = player.Rate,
                    NormalizedName = player.NormalizedName
                }).ToList()
            };
        }

        private void RefreshWithGameServerStatus(IQueryResponse queryResponse)
        {
            var newPlayers = new List<IGameServerPlayer>();

            foreach (var queryPlayer in queryResponse.Players)
            {
                var newPlayer = new GameServerPlayer(queryPlayer);

                var existing = (GameServerPlayer) Players.SingleOrDefault(player => player.NormalizedName == queryPlayer.NormalizedName);

                if (existing != null)
                    newPlayer.RconPlayer = existing.RconPlayer;

                newPlayers.Add(newPlayer);
            }

            Players = newPlayers;
        }

        private void RefreshWithRconPlayers(IEnumerable<IRconPlayer> rconPlayers)
        {
            foreach (var rconPlayer in rconPlayers)
            {
                var existingPlayer = (GameServerPlayer) Players.SingleOrDefault(player => player.NormalizedName == rconPlayer.NormalizedName);

                if (existingPlayer != null)
                {
                    if (existingPlayer.RconPlayer == null)
                        existingPlayer.RconPlayer = rconPlayer;
                }
                else
                {
                    _logger.LogDebug("Could not find Query Player to sync with Rcon Player with name {username}", rconPlayer.NormalizedName);
                }
            }
        }
    }
}