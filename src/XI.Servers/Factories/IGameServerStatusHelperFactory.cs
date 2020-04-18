using System;
using XI.CommonTypes;
using XI.Servers.Helpers;

namespace XI.Servers.Factories
{
    public interface IGameServerStatusHelperFactory
    {
        IGameServerStatusHelper GetGameServerStatusHelper(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
    }
}