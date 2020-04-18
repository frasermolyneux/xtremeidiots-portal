using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Servers.Rcon.Clients;

namespace XI.Servers.Rcon.Factories
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
            return CreateInstance(gameType, serverId, hostname, queryPort, rconPassword, null);
        }

        public IRconClient CreateInstance(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword, List<TimeSpan> retryOverride)
        {
            IRconClient rconClient;

            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    rconClient = new Cod2RconClient(_logger);
                    break;
                case GameType.CallOfDuty4:
                    rconClient = new Cod4RconClient(_logger);
                    break;
                case GameType.CallOfDuty5:
                    rconClient = new Cod5RconClient(_logger);
                    break;
                case GameType.Insurgency:
                    rconClient = new SourceRconClient(_logger);
                    break;
                default:
                    throw new Exception("Unsupported game type");
            }

            rconClient.Configure(serverId, hostname, queryPort, rconPassword, retryOverride);
            return rconClient;
        }
    }
}