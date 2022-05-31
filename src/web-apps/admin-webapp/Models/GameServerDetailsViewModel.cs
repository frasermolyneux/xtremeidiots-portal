using System.Collections.Generic;

namespace XtremeIdiots.Portal.AdminWebApp.Models
{
    public class GameServerDetailsViewModel
    {
        public GameServerViewModel GameServerViewModel { get; set; }
        public List<BanFileMonitorViewModel> BanFileMonitors { get; set; }
    }
}