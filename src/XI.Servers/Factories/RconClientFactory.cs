using Microsoft.Extensions.Logging;
using XI.Servers.Clients;
using XI.Servers.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Servers.Factories
{
    public class RconClientFactory : IRconClientFactory
    {
        private readonly ILogger<RconClientFactory> _logger;

        public RconClientFactory(ILogger<RconClientFactory> logger)
        {
            _logger = logger;
        }

        public IRconClient CreateInstance(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword)
        {
            IRconClient rconClient;

            switch (gameType)
            {
                case GameType.CallOfDuty2:
                case GameType.CallOfDuty4:
                case GameType.CallOfDuty5:
                    rconClient = new Quake3RconClient(_logger);
                    break;
                case GameType.Insurgency:
                case GameType.Rust:
                case GameType.Left4Dead2:
                    rconClient = new SourceRconClient(_logger);
                    break;
                default:
                    throw new Exception("Unsupported game type");
            }

            rconClient.Configure(gameType, serverId, hostname, queryPort, rconPassword);
            return rconClient;
        }
    }
}