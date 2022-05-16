using XtremeIdiots.Portal.ServersWebApi.Interfaces;

namespace XtremeIdiots.Portal.ServersWebApi.Models
{
    internal class SourceQueryPlayer : IQueryPlayer
    {
        public TimeSpan Time { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }
}