using System;
using System.Collections.Generic;
using XI.Servers.Interfaces;
using XI.Servers.Interfaces.Models;

namespace XI.Servers.Models
{
    internal class Quake3QueryResponse : IQueryResponse
    {
        public Quake3QueryResponse(Dictionary<string, string> serverParams, List<IQueryPlayer> players)
        {
            ServerParams = serverParams;
            Players = players;
        }

        public string ServerName => ServerParams.ContainsKey("sv_hostname") ? ServerParams["sv_hostname"] : null;

        public string Map => ServerParams.ContainsKey("mapname") ? ServerParams["mapname"] : null;
        public string Mod => ServerParams.ContainsKey("fs_game") ? ServerParams["fs_game"] : null;
        public int MaxPlayers => ServerParams.ContainsKey("sv_maxclients") ? Convert.ToInt32(ServerParams["sv_maxclients"]) : 0;

        public int PlayerCount => Players.Count;

        public IDictionary<string, string> ServerParams { get; set; }
        public IList<IQueryPlayer> Players { get; set; }
    }
}