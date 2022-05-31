using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class PlayerDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public string Username { get; set; } = string.Empty;

        [JsonProperty]
        public string Guid { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime FirstSeen { get; set; }

        [JsonProperty]
        public DateTime LastSeen { get; set; }

        [JsonProperty]
        public string IpAddress { get; set; } = string.Empty;
    }
}