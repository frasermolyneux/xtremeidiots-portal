using System.Threading.Tasks;
using XI.Servers.Models;

namespace XI.Servers.Interfaces
{
    public interface IGameServerClient
    {
        void Configure(string hostname, int queryPort);
        Task<IGameServerStatus> GetServerStatus();
    }
}