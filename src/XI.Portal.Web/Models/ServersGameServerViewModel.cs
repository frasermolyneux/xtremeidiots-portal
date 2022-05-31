using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XI.Portal.Web.Models
{
    public class ServersGameServerViewModel
    {
        public GameServerDto GameServer { get; set; }
        public MapDto Map { get; set; }
        public List<MapDto> Maps { get; set; } = new List<MapDto>();
        public List<GameServerStatDto> GameServerStats { get; set; } = new List<GameServerStatDto>();
        public List<LivePlayerDto> LivePlayers { get; set; } = new List<LivePlayerDto>();
        public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; } = new List<MapTimelineDataPoint>();
    }
}
