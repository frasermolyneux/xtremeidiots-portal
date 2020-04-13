using System;
using System.Collections.Generic;
using XI.Servers.Query.Models;

namespace XI.Servers.Models
{
    internal class GameServerStatus : IGameServerStatus
    {
        private readonly IQueryResponse _queryResponse;

        public GameServerStatus(IQueryResponse queryResponse, IList<IGameServerPlayer> players)
        {
            _queryResponse = queryResponse ?? throw new ArgumentNullException(nameof(queryResponse));
            Players = players;
        }

        public string ServerName => _queryResponse.ServerName;
        public string Map => _queryResponse.Map;
        public string Mod => _queryResponse.Mod;
        public int PlayerCount => _queryResponse.PlayerCount;
        public IList<IGameServerPlayer> Players { get; }
    }
}