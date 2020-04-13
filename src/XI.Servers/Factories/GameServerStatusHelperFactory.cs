using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Servers.Helpers;
using XI.Servers.Query.Factories;
using XI.Servers.Rcon.Factories;

namespace XI.Servers.Factories
{
    public class GameServerStatusHelperFactory : IGameServerStatusHelperFactory
    {
        private readonly ILogger<GameServerStatusHelperFactory> _logger;
        private readonly IQueryClientFactory _queryClientFactory;
        private readonly IRconClientFactory _rconClientFactory;

        private readonly Dictionary<string, IGameServerStatusHelper> _instances = new Dictionary<string, IGameServerStatusHelper>();

        public GameServerStatusHelperFactory(ILogger<GameServerStatusHelperFactory> logger, IQueryClientFactory queryClientFactory, IRconClientFactory rconClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryClientFactory = queryClientFactory ?? throw new ArgumentNullException(nameof(queryClientFactory));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        public IGameServerStatusHelper GetGameServerStatusHelper(GameType gameType, string serverName, string hostname, int queryPort, string rconPassword)
        {
            if (_instances.ContainsKey(serverName)) return _instances[serverName];

            IGameServerStatusHelper gameServerStatusHelper = new GameServerStatusHelper(_logger, _queryClientFactory, _rconClientFactory);
            gameServerStatusHelper.Configure(gameType, serverName, hostname, queryPort, rconPassword);

            _instances.Add(serverName, gameServerStatusHelper);

            return gameServerStatusHelper;
        }
    }
}