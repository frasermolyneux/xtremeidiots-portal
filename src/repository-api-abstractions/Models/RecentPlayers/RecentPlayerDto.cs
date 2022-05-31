using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers
{
    public class RecentPlayerDto
    {
        [JsonProperty]
        public Guid Id { get; internal set; }
        [JsonProperty]
        public string? Name { get; internal set; }
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
        public Guid? ServerId { get; internal set; }
        [JsonProperty]
        public DateTime Timestamp { get; internal set; }
    }
}
