using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Servers.Models
{
    public class GameServerStatusFilterModel
    {
        public List<GameType> GameTypes { get; set; }
        public List<Guid> ServerIds { get; set; }
    }
}