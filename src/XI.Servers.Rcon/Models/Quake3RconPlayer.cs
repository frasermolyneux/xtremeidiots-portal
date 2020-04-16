using System.Collections.Generic;
using System.Linq;

namespace XI.Servers.Rcon.Models
{
    internal class Quake3RconPlayer : IRconPlayer
    {
        public string Score { get; set; }
        public string Ping { get; set; }
        public string QPort { get; set; }
        public string Num { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Rate { get; set; }

        public string NormalizedName
        {
            get
            {
                var toRemove = new List<string> {"^0", "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9"};

                var toReturn = Name.ToUpper();
                toReturn = toRemove.Aggregate(toReturn, (current, val) => current.Replace(val, ""));
                toReturn = toReturn.Trim();
                return toReturn;
            }
        }
    }
}