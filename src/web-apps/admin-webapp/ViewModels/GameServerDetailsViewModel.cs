using System.Collections.Generic;

using XtremeIdiots.Portal.AdminWebApp.Models;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class GameServerDetailsViewModel
    {
        public GameServerViewModel GameServerViewModel { get; set; }
        public List<BanFileMonitorViewModel> BanFileMonitors { get; set; }
    }
}