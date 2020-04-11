using System.Collections.Generic;
using XI.Portal.Data.Legacy.Models;

namespace XI.Portal.Players.Models
{
    public class PlayerInfoViewModel
    {
        public Player2 Player { get; set; }
        //public LookupAddressResponse LookupAddressResponse { get; set; }
        public List<PlayerAlias> Aliases { get; set; }
        public List<PlayerIpAddresses> IpAddresses { get; set; }
        public List<AdminActions> AdminActions { get; set; }
        public List<PlayerIpAddresses> RelatedIpAddresses { get; set; }
    }
}