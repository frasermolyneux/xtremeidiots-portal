
using XtremeIdiots.Portal.ServersApiClient.Interfaces;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public class ServersApiClient : IServersApiClient
    {
        public ServersApiClient(
            IQueryApi queryApiClient,
            IRconApi rconApiClient)
        {
            Query = queryApiClient;
            Rcon = rconApiClient;
        }

        public IQueryApi Query { get; }
        public IRconApi Rcon { get; }
    }
}