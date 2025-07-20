using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class ServersGameServerViewModel(GameServerDto gameServer)
{
    public GameServerDto GameServer { get; private set; } = gameServer;
    public MapDto? Map { get; set; }
    public List<MapDto> Maps { get; set; } = [];
    public List<GameServerStatDto> GameServerStats { get; set; } = [];
    public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; } = [];
}