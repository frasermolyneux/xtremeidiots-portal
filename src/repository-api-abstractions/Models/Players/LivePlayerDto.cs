using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class LivePlayerDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public string? Name { get; set; }

        [JsonProperty]
        public int Score { get; set; }

        [JsonProperty]
        public int Ping { get; set; }

        [JsonProperty]
        public int Num { get; set; }

        [JsonProperty]
        public int Rate { get; set; }

        [JsonProperty]
        public string? Team { get; set; }

        [JsonProperty]
        public TimeSpan Time { get; set; }

        [JsonProperty]
        public string? IpAddress { get; set; }

        [JsonProperty]
        public double? Lat { get; set; }

        [JsonProperty]
        public double? Long { get; set; }

        [JsonProperty]
        public string? CountryCode { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public Guid? PlayerId { get; set; }

        [JsonProperty]
        public Guid? GameServerServerId { get; set; }
    }
}
