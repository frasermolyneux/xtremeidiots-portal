using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for displaying game server information in the servers section
/// </summary>
/// <param name="gameServer">The game server data transfer object</param>
public class ServersGameServerViewModel(GameServerDto gameServer)
{
    /// <summary>
    /// Gets the game server data
    /// </summary>
    public GameServerDto GameServer { get; private set; } = gameServer;

    /// <summary>
    /// Gets or sets the current map being played on the server
    /// </summary>
    public MapDto? Map { get; set; }

    /// <summary>
    /// Gets or sets the list of available maps for the server
    /// </summary>
    public List<MapDto> Maps { get; set; } = [];

    /// <summary>
    /// Gets or sets the server statistics
    /// </summary>
    public List<GameServerStatDto> GameServerStats { get; set; } = [];

    /// <summary>
    /// Gets or sets the map timeline data points for visualization
    /// </summary>
    public List<MapTimelineDataPoint> MapTimelineDataPoints { get; set; } = [];
}