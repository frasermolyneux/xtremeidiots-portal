using System;
using System.Collections.Generic;
using XI.CommonTypes;
using XI.Servers.Interfaces.Models;

namespace XI.Servers.Dto
{
    public class GameServerStatusDto : IGameServerStatus<GameServerPlayerDto>
    {
        public Guid ServerId { get; set; }
        public GameType GameType { get; set; }
        public string Hostname { get; set; }
        public int QueryPort { get; set; }
        public string ServerName { get; set; }
        public string Map { get; set; }
        public string Mod { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public IList<GameServerPlayerDto> Players { get; set; }
    }
}