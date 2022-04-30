using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XI.Portal.Web.Models
{
    public class GameServerDetailsViewModel
    {
        public GameServerDto GameServerDto { get; set; }
        public List<BanFileMonitorViewModel> BanFileMonitors { get; set; }
    }
}