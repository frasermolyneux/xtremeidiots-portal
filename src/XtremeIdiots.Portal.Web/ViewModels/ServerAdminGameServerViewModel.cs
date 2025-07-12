using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class ServerAdminGameServerViewModel
    {
        public GameServerDto GameServer { get; set; }
        public ServerQueryStatusResponseDto GameServerQueryStatus { get; set; }
        public ServerRconStatusResponseDto GameServerRconStatus { get; set; }
    }
}
