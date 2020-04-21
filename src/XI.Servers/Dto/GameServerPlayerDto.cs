using XI.Servers.Interfaces.Models;

namespace XI.Servers.Dto
{
    public class GameServerPlayerDto : IGameServerPlayer
    {
        public string Num { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Score { get; set; }
        public string Rate { get; set; }
        public string NormalizedName { get; set; }
    }
}