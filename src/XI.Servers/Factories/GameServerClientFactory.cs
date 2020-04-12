using System;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Servers.Clients;
using XI.Servers.Interfaces;

namespace XI.Servers.Factories
{
    public class GameServerClientFactory : IGameServerClientFactory
    {
        private readonly ILogger<GameServerClientFactory> _logger;

        public GameServerClientFactory(ILogger<GameServerClientFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IGameServerClient CreateInstance(GameType gameType, string hostname, int queryPort)
        {
            IGameServerClient gameServerClient;

            switch (gameType)
            {
                case GameType.CallOfDuty2:
                case GameType.CallOfDuty4:
                case GameType.CallOfDuty5:
                    gameServerClient = new Quake3Client(_logger);
                    break;
                case GameType.Insurgency:
                case GameType.Rust:
                case GameType.Left4Dead2:
                    gameServerClient = new SourceClient(_logger);
                    break;
                default:
                    throw new Exception("Unsupported game type");
            }

            gameServerClient.Configure(hostname, queryPort);
            return gameServerClient;
        }
    }
}