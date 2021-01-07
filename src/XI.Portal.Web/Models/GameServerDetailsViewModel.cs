using System.Collections.Generic;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Web.Models
{
    public class GameServerDetailsViewModel
    {
        public GameServerDto GameServerDto { get; set; }
        public List<BanFileMonitorDto> BanFileMonitorDtos { get; set; }
    }
}