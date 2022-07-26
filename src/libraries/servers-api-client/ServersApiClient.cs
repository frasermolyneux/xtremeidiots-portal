
using XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public class ServersApiClient : IServersApiClient
    {
        public ServersApiClient(
            IQueryApi queryApiClient,
            IRconApi rconApiClient,
            IMapsApi mapsApiClient)
        {
            Query = queryApiClient;
            Rcon = rconApiClient;
            Maps = mapsApiClient;
        }

        public IQueryApi Query { get; }
        public IRconApi Rcon { get; }
        public IMapsApi Maps { get; }
    }
}