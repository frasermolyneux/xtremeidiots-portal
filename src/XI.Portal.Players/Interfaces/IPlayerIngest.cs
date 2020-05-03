using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;

namespace XI.Portal.Players.Interfaces
{
    public interface IPlayerIngest
    {
        Task IngestData(GameType gameType, string guid, string username, string ipAddress);
        void OverrideLogger(ILogger logger);
    }
}