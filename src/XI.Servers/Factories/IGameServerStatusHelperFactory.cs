using XI.CommonTypes;
using XI.Servers.Helpers;

namespace XI.Servers.Factories
{
    public interface IGameServerStatusHelperFactory
    {
        IGameServerStatusHelper GetGameServerStatusHelper(GameType gameType, string serverName, string hostname, int queryPort, string rconPassword);
    }
}