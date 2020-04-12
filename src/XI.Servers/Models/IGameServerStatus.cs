using System.Collections.Generic;

namespace XI.Servers.Models
{
    public interface IGameServerStatus
    {
        string ServerName { get; }
        string Map { get; }
        string Mod { get; }
        int PlayerCount { get; }

        Dictionary<string, string> Params { get; set; }
        List<LivePlayer> Players { get; set; }
    }
}