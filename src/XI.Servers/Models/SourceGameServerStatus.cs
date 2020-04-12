using System.Collections.Generic;

namespace XI.Servers.Models
{
    public class SourceGameServerStatus : IGameServerStatus
    {
        public string ServerName => Params.ContainsKey("hostname") ? Params["hostname"] : null;

        public string Map => Params.ContainsKey("mapname") ? Params["mapname"] : null;
        public string Mod => Params.ContainsKey("modname") ? Params["modname"] : null;

        public int PlayerCount => Players.Count;

        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
        public List<LivePlayer> Players { get; set; } = new List<LivePlayer>();
    }
}