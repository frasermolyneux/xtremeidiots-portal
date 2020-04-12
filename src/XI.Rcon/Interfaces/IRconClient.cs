using System;
using System.Collections.Generic;

namespace XI.Rcon.Interfaces
{
    public interface IRconClient
    {
        void Configure(string serverName, string hostname, int queryPort, string rconPassword, List<TimeSpan> retryOverride);
        string PlayerStatus();
        string KickPlayer(string targetPlayerNum);
        string BanPlayer(string targetPlayerNum);
        string RestartServer();
        string RestartMap();
        string NextMap();
        string MapRotation();
        string Say(string message);
    }
}