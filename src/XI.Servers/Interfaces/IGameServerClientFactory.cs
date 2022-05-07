using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Servers.Interfaces
{
    public interface IGameServerClientFactory
    {
        IGameServerClient GetGameServerStatusHelper(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
    }
}