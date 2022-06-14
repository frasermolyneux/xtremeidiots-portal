using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class LivePlayerDto
    {
        [JsonProperty]
        public Guid LivePlayerId { get; internal set; }

        [JsonProperty]
        public string? Name { get; internal set; }

        [JsonProperty]
        public int Score { get; internal set; }

        [JsonProperty]
        public int Ping { get; internal set; }

        [JsonProperty]
        public int Num { get; internal set; }

        [JsonProperty]
        public int Rate { get; internal set; }

        [JsonProperty]
        public string? Team { get; internal set; }

        [JsonProperty]
        public TimeSpan Time { get; internal set; }

        [JsonProperty]
        public string? IpAddress { get; internal set; }

        [JsonProperty]
        public double? Lat { get; internal set; }

        [JsonProperty]
        public double? Long { get; internal set; }

        [JsonProperty]
        public string? CountryCode { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; internal set; }

        [JsonProperty]
        public Guid? PlayerId { get; internal set; }

        [JsonProperty]
        public Guid? GameServerServerId { get; internal set; }

        [JsonProperty]
        public PlayerDto? Player { get; internal set; }
    }
}
