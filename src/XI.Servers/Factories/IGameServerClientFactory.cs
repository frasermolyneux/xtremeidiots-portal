using XI.CommonTypes;
using XI.Servers.Interfaces;

namespace XI.Servers.Factories
{
    public interface IGameServerClientFactory
    {
        IGameServerClient CreateInstance(GameType gameType, string hostname, int queryPort);
    }
}