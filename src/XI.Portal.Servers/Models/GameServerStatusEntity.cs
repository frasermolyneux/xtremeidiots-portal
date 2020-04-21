using System;
using System.Collections.Generic;
using XI.AzureTableExtensions;
using XI.AzureTableExtensions.Attributes;
using XI.CommonTypes;
using XI.Servers.Dto;

namespace XI.Portal.Servers.Models
{
    internal class GameServerStatusEntity : TableEntityExtended
    {
        public GameServerStatusEntity()
        {
        }

        public GameServerStatusEntity(Guid serverId, GameServerStatusDto gameServerStatus)
        {
            RowKey = serverId.ToString();
            PartitionKey = "status";

            Hostname = gameServerStatus.Hostname;
            QueryPort = gameServerStatus.QueryPort;
            ServerId = gameServerStatus.ServerId;
            GameType = gameServerStatus.GameType;
            ServerName = gameServerStatus.ServerName;
            Map = gameServerStatus.Map;
            Mod = gameServerStatus.Mod;
            PlayerCount = gameServerStatus.PlayerCount;
            MaxPlayers = gameServerStatus.MaxPlayers;
            Players = gameServerStatus.Players;
        }

        public string Hostname { get; set; }
        public int QueryPort { get; set; }
        public string ServerName { get; set; }
        public string Map { get; set; }
        public string Mod { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }

        [EntityJsonPropertyConverter] public IList<GameServerPlayerDto> Players { get; set; }
        public Guid ServerId { get; set; }

        [EntityEnumPropertyConverter] public GameType GameType { get; set; }
    }
}