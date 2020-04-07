using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class LivePlayerLocations
    {
        public Guid LivePlayerLocationId { get; set; }
        public string IpAddress { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public DateTime LastSeen { get; set; }
    }
}