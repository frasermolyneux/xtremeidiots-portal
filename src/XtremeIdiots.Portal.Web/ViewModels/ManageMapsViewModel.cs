using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1.Rcon;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class ManageMapsViewModel(GameServerDto gameServer)
{
    public GameServerDto GameServer { get; private set; } = gameServer;
    public List<MapDto> Maps { get; set; } = [];
    public List<ServerMapDto> ServerMaps { get; set; } = [];
    public List<RconMapDto> RconMaps { get; set; } = [];
    public List<MapPackDto> MapPacks { get; set; } = [];
}