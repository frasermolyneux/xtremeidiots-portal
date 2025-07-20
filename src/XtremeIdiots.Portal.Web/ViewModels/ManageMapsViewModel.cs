using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1.Rcon;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class ManageMapsViewModel
    {
        public ManageMapsViewModel(GameServerDto gameServer)
        {
            GameServer = gameServer;
        }

        public GameServerDto GameServer { get; private set; }
        public List<MapDto> Maps { get; set; } = new List<MapDto>();
        public List<ServerMapDto> ServerMaps { get; set; } = new List<ServerMapDto>();
        public List<RconMapDto> RconMaps { get; set; } = new List<RconMapDto>();
        public List<MapPackDto> MapPacks { get; set; } = new List<MapPackDto>();
    }
}