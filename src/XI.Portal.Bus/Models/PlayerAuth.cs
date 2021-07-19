using System;
using XI.CommonTypes;

namespace XI.Portal.Bus.Models
{
    public class PlayerAuth
    {
        public GameType GameType { get; set; }
        public Guid ServerId { get; set; }
        public string Guid { get; set; }
        public string Username { get; set; }
        public string IpAddress { get; set; }
    }
}