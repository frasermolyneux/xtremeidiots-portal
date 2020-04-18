using System;
using System.Collections.Generic;
using XI.Servers.Rcon.Models;

namespace XI.Servers.Rcon.Clients
{
    public interface IRconClient
    {
        void Configure(Guid serverId, string hostname, int queryPort, string rconPassword, List<TimeSpan> retryOverride);
        List<IRconPlayer> GetPlayers();
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