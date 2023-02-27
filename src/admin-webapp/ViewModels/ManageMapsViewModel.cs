using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models.Maps;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class ManageMapsViewModel
    {
        public ManageMapsViewModel(GameServerDto gameServer)
        {
            GameServer = gameServer;
        }

        public GameServerDto GameServer { get; private set; }
        public List<MapDto> Maps { get; set; } = new List<MapDto>();
        public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; } = new List<MapTimelineDataPoint>();
        public List<ServerMapDto> ServerMaps { get; set; } = new List<ServerMapDto>();
    }
}
