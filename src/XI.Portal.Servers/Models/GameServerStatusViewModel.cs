using XI.Portal.Servers.Dto;

ls;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Models
{
    public class GameServerStatusViewModel
    {
        public GameServerDto GameServer { get; set; }

        public string Mod { get; set; }
        public string Map { get; set; }
        public int PlayerCount { get; set; }

        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public string SuccessMessage {
    set; } = "Everything looks OK";
    }
}