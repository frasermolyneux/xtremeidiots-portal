using System;
using XI.CommonTypes;

namespace XI.Portal.Players.Dto
{
    public class PlayerDto
    {
        public Guid PlayerId { get; set; }
        public GameType GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        public string IpAddress { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }
}