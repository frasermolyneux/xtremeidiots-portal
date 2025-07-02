using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class ServersGameServerViewModel
    {
        public ServersGameServerViewModel(GameServerDto gameServer)
        {
            GameServer = gameServer;
        }

        public GameServerDto GameServer { get; private set; }
        public MapDto? Map { get; set; }
        public List<MapDto> Maps { get; set; } = new List<MapDto>();
        public List<GameServerStatDto> GameServerStats { get; set; } = new List<GameServerStatDto>();
        public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; } = new List<MapTimelineDataPoint>();
    }
}
