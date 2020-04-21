using XI.Servers.Interfaces.Models;

namespace XI.Servers.Models
{
    internal class Quake3QueryPlayer : IQueryPlayer
    {
        public int Ping { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }

        public string NormalizedName => Name.Normalize();
    }
}