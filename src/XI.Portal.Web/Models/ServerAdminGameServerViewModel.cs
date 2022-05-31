using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.ServersApi.Abstractions.Models;

namespace XI.Portal.Web.Models
{
    public class ServerAdminGameServerViewModel
    {
        public GameServerDto GameServer { get; set; }
        public ServerQueryStatusResponseDto GameServerQueryStatus { get; set; }
        public ServerRconStatusResponseDto GameServerRconStatus { get; set; }
    }
}
