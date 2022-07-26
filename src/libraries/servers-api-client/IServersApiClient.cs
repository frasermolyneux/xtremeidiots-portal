using XtremeIdiots.Portal.ServersApi.Abstractions.Interfaces;

namespace XtremeIdiots.Portal.ServersApiClient
{
    public interface IServersApiClient
    {
        public IQueryApi Query { get; }
        public IRconApi Rcon { get; }
        public IMapsApi Maps { get; }
    }
}