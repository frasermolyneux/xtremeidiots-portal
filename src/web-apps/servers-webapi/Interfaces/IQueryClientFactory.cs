using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.ServersWebApi.Interfaces
{
    public interface IQueryClientFactory
    {
        IQueryClient CreateInstance(GameType gameType, string hostname, int queryPort);
    }
}