using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Models
{
    public class RconMonitorStatusViewModel
    {
        public RconMonitors RconMonitor { get; set; }
        public GameServers GameServer { get; set; }

        public string RconStatusResult { get; set; }

        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public string SuccessMessage { get; set; } = "Everything looks OK";
    }
}