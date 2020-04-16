using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Servers.Models;
using XI.Servers.Query.Clients;
using XI.Servers.Query.Factories;
using XI.Servers.Query.Models;
using XI.Servers.Rcon.Clients;
using XI.Servers.Rcon.Factories;
using XI.Servers.Rcon.Models;

namespace XI.Servers.Helpers
{
    internal class GameServerStatusHelper : IGameServerStatusHelper
    {
        private readonly ILogger _logger;
        private readonly IQueryClientFactory _queryClientFactory;
        private readonly IRconClientFactory _rconClientFactory;

        private DateTime _lastRconUpdate = DateTime.UtcNow;

        public GameServerStatusHelper(ILogger logger, IQueryClientFactory queryClientFactory, IRconClientFactory rconClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryClientFactory = queryClientFactory ?? throw new ArgumentNullException(nameof(queryClientFactory));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        private IQueryClient QueryClient { get; set; }
        private IRconClient RconClient { get; set; }

        public List<IGameServerPlayer> Players { get; set; } = new List<IGameServerPlayer>();

        public void Configure(GameType gameType, string serverName, string hostname, int queryPort, string rconPassword)
        {
            QueryClient = _queryClientFactory.CreateInstance(gameType, hostname, queryPort);
            RconClient = _rconClientFactory.CreateInstance(gameType, serverName, hostname, queryPort, rconPassword);
        }

        public async Task<IGameServerStatus> GetServerStatus()
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

            if (needsRconSync)
            {
                var rconPlayers = RconClient.GetPlayers();
                RefreshWithRconPlayers(rconPlayers);
                _lastRconUpdate = DateTime.UtcNow;
            }

            return new GameServerStatus(queryResponse, Players);
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
                    _logger.LogWarning("Could not find Query Player to sync with Rcon Player");
                }
            }
        }
    }
}