using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Models
{
    internal class SourceRconPlayer : IRconPlayer
    {
        public int Ping { get; set; }
        public int Num { get; set; }
        public string? Guid { get; set; }
        public string? Name { get; set; }
        public string? IpAddress { get; set; }
        public int Rate { get; set; }
    }
}