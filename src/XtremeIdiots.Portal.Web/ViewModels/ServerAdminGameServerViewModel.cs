using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class ServerAdminGameServerViewModel
{
    public required GameServerDto GameServer { get; set; }
    public required ServerQueryStatusResponseDto GameServerQueryStatus { get; set; }
    public required ServerRconStatusResponseDto GameServerRconStatus { get; set; }
}