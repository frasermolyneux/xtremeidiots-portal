
using XtremeIdiots.Portal.ServersApiClient.QueryApi;
using XtremeIdiots.Portal.ServersApiClient.RconApi;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public class ServersApiClient : IServersApiClient
    {
        public ServersApiClient(
            IQueryApiClient queryApiClient,
            IRconApiClient rconApiClient)
        {
            Query = queryApiClient;
            Rcon = rconApiClient;
        }

        public IQueryApiClient Query { get; }
        public IRconApiClient Rcon { get; }
    }
}