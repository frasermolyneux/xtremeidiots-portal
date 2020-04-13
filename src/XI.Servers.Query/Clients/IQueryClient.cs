using System.Threading.Tasks;
using XI.Servers.Query.Models;

namespace XI.Servers.Query.Clients
{
    public interface IQueryClient
    {
        void Configure(string hostname, int queryPort);
        Task<IQueryResponse> GetServerStatus();
    }
}