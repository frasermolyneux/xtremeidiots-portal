using System.Collections.Generic;
using System.Linq;

namespace XI.Servers.Query.Models
{
    internal class Quake3QueryPlayer : IQueryPlayer
    {
        public int Ping { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }

        public string NormalizedName
        {
            get
            {
                var toRemove = new List<string> { "^0", "^1", "^2", "^3", "^4", "^5", "^6", "^7", "^8", "^9" };

                var toReturn = Name.ToUpper();
                toReturn = toRemove.Aggregate(toReturn, (current, val) => current.Replace(val, ""));
                toReturn = toReturn.Trim();
                return toReturn;
            }
        }
    }
}