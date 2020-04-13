using XI.CommonTypes;
using XI.Servers.Query.Clients;

namespace XI.Servers.Query.Factories
{
    public interface IQueryClientFactory
    {
        IQueryClient CreateInstance(GameType gameType, string hostname, int queryPort);
    }
}