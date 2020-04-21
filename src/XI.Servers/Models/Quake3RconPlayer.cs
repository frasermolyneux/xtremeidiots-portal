using XI.Servers.Interfaces.Models;

namespace XI.Servers.Models
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

        public string NormalizedName => Name.Normalize();
    }
}