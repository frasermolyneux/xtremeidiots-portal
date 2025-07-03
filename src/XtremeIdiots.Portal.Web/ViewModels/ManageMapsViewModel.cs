using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1.Rcon;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.MapPacks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps;

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
