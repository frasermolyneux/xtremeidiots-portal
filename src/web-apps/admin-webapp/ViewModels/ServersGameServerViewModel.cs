using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
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
        public List<LivePlayerDto> LivePlayers { get; set; } = new List<LivePlayerDto>();
        public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; } = new List<MapTimelineDataPoint>();
    }
}
