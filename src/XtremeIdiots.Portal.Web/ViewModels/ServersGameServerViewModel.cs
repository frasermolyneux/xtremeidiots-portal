using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;

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
