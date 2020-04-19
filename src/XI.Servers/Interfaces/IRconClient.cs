using System;
using System.Collections.Generic;
using XI.CommonTypes;
using XI.Servers.Interfaces.Models;

namespace XI.Servers.Interfaces
{
    public interface IRconClient
    {
        void Configure(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
        List<IRconPlayer> GetPlayers();
        //string PlayerStatus();
        //string KickPlayer(string targetPlayerNum);
        //string BanPlayer(string targetPlayerNum);
        //string RestartServer();
        //string RestartMap();
        //string NextMap();
        //string MapRotation();
        //string Say(string message);
    }
}