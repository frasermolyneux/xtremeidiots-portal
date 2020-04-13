using System;
using System.Collections.Generic;
using XI.CommonTypes;
using XI.Servers.Rcon.Clients;

namespace XI.Servers.Rcon.Factories
{
    public interface IRconClientFactory
    {
        IRconClient CreateInstance(GameType gameType, string serverName, string hostname, int queryPort, string rconPassword);
        IRconClient CreateInstance(GameType gameType, string title, string hostname, int queryPort, string rconPassword, List<TimeSpan> retryOverride);
    }
}