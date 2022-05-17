using XI.Servers.Extensions;
using XI.Servers.Interfaces.Models;

namespace XI.Servers.Models
{
    internal class SourceQueryPlayer : IQueryPlayer
    {
        public TimeSpan Time { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }

        public string NormalizedName => Name.NormalizeName();
    }
}