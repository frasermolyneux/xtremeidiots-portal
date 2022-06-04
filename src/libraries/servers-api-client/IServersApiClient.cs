using XtremeIdiots.Portal.ServersApiClient.Interfaces;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public interface IServersApiClient
    {
        public IQueryApi Query { get; }
        public IRconApi Rcon { get; }
    }
}