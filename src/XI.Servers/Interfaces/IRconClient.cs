using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Servers.Interfaces.Models;

namespace XI.Servers.Interfaces
{
    public interface IRconClient
    {
        void Configure(GameType gameType, Guid serverId, string hostname, int queryPort, string rconPassword);
        List<IRconPlayer> GetPlayers();
        Task Say(string message);
        Task<string> Restart();
        Task<string> RestartMap();
        Task<string> FastRestartMap();
        Task<string> NextMap();
    }
}