using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayerIngest
    {
        Task IngestData(GameType gameType, string guid, string username, string ipAddress);
        void OverrideLogger(ILogger logger);
    }
}