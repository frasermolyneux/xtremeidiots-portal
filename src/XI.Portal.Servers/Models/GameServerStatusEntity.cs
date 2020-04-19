using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using XI.CommonTypes;
using XI.Portal.Servers.Attributes;
using XI.Servers.Dto;

namespace XI.Portal.Servers.Models
{
    internal class GameServerStatusEntity : TableEntity
    {
        public GameServerStatusEntity()
        {
        }

        public GameServerStatusEntity(Guid serverId, GameServerStatusDto gameServerStatus)
        {
            RowKey = serverId.ToString();
            PartitionKey = "status";

            ServerId = gameServerStatus.ServerId;
            GameType = gameServerStatus.GameType;
            ServerName = gameServerStatus.ServerName;
            Map = gameServerStatus.Map;
            Mod = gameServerStatus.Mod;
            PlayerCount = gameServerStatus.PlayerCount;
            Players = gameServerStatus.Players;
        }

        public string ServerName { get; set; }
        public string Map { get; set; }
        public string Mod { get; set; }
        public int PlayerCount { get; set; }

        [EntityJsonPropertyConverter] public IList<GameServerPlayerDto> Players { get; set; }
        public Guid ServerId { get; set; }

        [EntityEnumPropertyConverter]
        public GameType GameType { get; set; }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            EntityJsonPropertyConverter.Serialize(this, results);
            EntityEnumPropertyConverter.Serialize(this, results);
            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            EntityJsonPropertyConverter.Deserialize(this, properties);
            EntityEnumPropertyConverter.Deserialize(this, properties);
        }
    }
}