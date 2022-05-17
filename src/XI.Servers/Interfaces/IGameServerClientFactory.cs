using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Servers.Interfaces
{
    public interface IGameServerClientFactory
    {
        IGameServerClient GetGameServerStatusHelper(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
    }
}