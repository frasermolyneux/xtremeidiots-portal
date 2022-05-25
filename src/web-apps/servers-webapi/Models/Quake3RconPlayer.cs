using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Models
{
    internal class Quake3RconPlayer : IRconPlayer
    {
        public int Score { get; set; }
        public int Ping { get; set; }
        public string? QPort { get; set; }
        public int Num { get; set; }
        public string? Guid { get; set; }
        public string? Name { get; set; }
        public string? IpAddress { get; set; }
        public int Rate { get; set; }
    }
}