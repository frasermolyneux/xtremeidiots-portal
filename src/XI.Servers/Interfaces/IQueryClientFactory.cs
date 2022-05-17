using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Servers.Interfaces
{
    public interface IQueryClientFactory
    {
        IQueryClient CreateInstance(GameType gameType, string hostname, int queryPort);
    }
}