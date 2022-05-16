using XtremeIdiots.Portal.ServersApiClient.QueryApi;
using XtremeIdiots.Portal.ServersApiClient.RconApi;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public interface IServersApiClient
    {
        public IQueryApiClient Query { get; }
        public IRconApiClient Rcon { get; }
    }
}