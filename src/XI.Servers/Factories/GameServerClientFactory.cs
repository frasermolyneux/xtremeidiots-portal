using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Servers.Clients;
using XI.Servers.Interfaces;

namespace XI.Servers.Factories
{
    public class GameServerClientFactory : IGameServerClientFactory
    {
        private readonly Dictionary<Guid, IGameServerClient> _instances = new Dictionary<Guid, IGameServerClient>();
        private readonly ILogger<GameServerClientFactory> _logger;
        private readonly IQueryClientFactory _queryClientFactory;
        private readonly IRconClientFactory _rconClientFactory;

        public GameServerClientFactory(ILogger<GameServerClientFactory> logger, IQueryClientFactory queryClientFactory, IRconClientFactory rconClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryClientFactory = queryClientFactory ?? throw new ArgumentNullException(nameof(queryClientFactory));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        public IGameServerClient GetGameServerStatusHelper(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword)
        {
            if (_instances.ContainsKey(serverId)) return _instances[serverId];

            IGameServerClient gameServerClient = new GameServerClient(_logger, _queryClientFactory, _rconClientFactory);
            gameServerClient.Configure(gameType, serverId, hostname, queryPort, rconPassword);

            _instances.Add(serverId, gameServerClient);

            return gameServerClient;
        }
    }
}