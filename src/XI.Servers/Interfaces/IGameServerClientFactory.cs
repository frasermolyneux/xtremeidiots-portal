using System;
using XI.CommonTypes;

namespace XI.Servers.Interfaces
{
    public interface IGameServerClientFactory
    {
        IGameServerClient GetGameServerStatusHelper(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
    }
}