using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for server administration containing game server details and status information
/// </summary>
public class ServerAdminGameServerViewModel
{
    /// <summary>
    /// Gets or sets the game server configuration and details
    /// </summary>
    public required GameServerDto GameServer { get; set; }

    /// <summary>
    /// Gets or sets the current query status of the game server
    /// </summary>
    public required ServerQueryStatusResponseDto GameServerQueryStatus { get; set; }

    /// <summary>
    /// Gets or sets the current RCON connection status of the game server
    /// </summary>
    public required ServerRconStatusResponseDto GameServerRconStatus { get; set; }
}