using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayerIngest
    {
        Task IngestData(GameType gameType, string guid, string username, string ipAddress);
        void OverrideLogger(ILogger logger);
    }
}