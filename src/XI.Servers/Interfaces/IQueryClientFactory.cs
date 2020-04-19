using XI.CommonTypes;

namespace XI.Servers.Interfaces
{
    public interface IQueryClientFactory
    {
        IQueryClient CreateInstance(GameType gameType, string hostname, int queryPort);
    }
}