using System;

namespace XI.Servers.Interfaces
{
    public interface IGameServerClientFactory
    {
        IGameServerClient GetGameServerStatusHelper(string gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
    }
}