using System;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Servers.Models
{
    public class BanFileMonitorStatusViewModel
    {
        public BanFileMonitors BanFileMonitor { get; set; }
        public GameServers GameServer { get; set; }

        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime LastGameBan { get; set; }

        public string ErrorMessage { get; set; }
        public string WarningMessage { get; set; }
        public string SuccessMessage { get; set; } = "Everything looks OK";
    }
}