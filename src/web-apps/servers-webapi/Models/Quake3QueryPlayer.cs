using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Models
{
    internal class Quake3QueryPlayer : IQueryPlayer
    {
        public int Ping { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }
}