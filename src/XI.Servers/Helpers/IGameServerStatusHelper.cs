using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Servers.Models;

namespace XI.Servers.Helpers
{
    public interface IGameServerStatusHelper
    {
        void Configure(GameType gameType, string serverName, string hostname, int queryPort, string rconPassword);
        Task<IGameServerStatus> GetServerStatus();
    }
}