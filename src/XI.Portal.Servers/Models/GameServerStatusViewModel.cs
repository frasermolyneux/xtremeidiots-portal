using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Models
{
    public class GameServerStatusViewModel
    {
        public GameServers GameServer { get; set; }

        public string Mod { get; set; }
        public string Map { get; set; }
        public int PlayerCount { get; set; }

        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public string SuccessMessage { get; set; } = "Everything looks OK";
    }
}