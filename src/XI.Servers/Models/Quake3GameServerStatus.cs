using System.Collections.Generic;

namespace XI.Servers.Models
{
    public class Quake3GameServerStatus : IGameServerStatus
    {
        public string ServerName => Params.ContainsKey("sv_hostname") ? Params["sv_hostname"] : null;

        public string Map => Params.ContainsKey("mapname") ? Params["mapname"] : null;
        public string Mod => Params.ContainsKey("fs_game") ? Params["fs_game"] : null;

        public int PlayerCount => Players.Count;

        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
        public List<LivePlayer> Players { get; set; } = new List<LivePlayer>();
    }
}