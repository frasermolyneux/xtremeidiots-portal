using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Models
{
    internal class SourceRconPlayer : IRconPlayer
    {
        public string Ping { get; set; }
        public string Num { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Rate { get; set; }
    }
}