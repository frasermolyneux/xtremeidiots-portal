﻿using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages
{
    public class ChatMessageDto
    {
        [JsonProperty]
        public Guid ChatLogId { get; internal set; }

        [JsonProperty]
        public Guid PlayerId { get; internal set; }

        [JsonProperty]
        public DateTime Timestamp { get; internal set; }

        [JsonProperty]
        public string Username { get; internal set; } = string.Empty;

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public ChatType ChatType { get; internal set; }

        [JsonProperty]
        public string Message { get; internal set; } = string.Empty;

        [JsonProperty]
        public PlayerDto? PlayerDto { get; internal set; }

        [JsonProperty]
        public GameServerDto? GameServerDto { get; internal set; }
    }
}
