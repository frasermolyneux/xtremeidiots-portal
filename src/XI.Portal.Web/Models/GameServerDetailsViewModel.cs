using System.Collections.Generic;

namespace XI.Portal.Web.Models
{
    public class GameServerDetailsViewModel
    {
        public GameServerViewModel GameServerViewModel { get; set; }
        public List<BanFileMonitorViewModel> BanFileMonitors { get; set; }
    }
}