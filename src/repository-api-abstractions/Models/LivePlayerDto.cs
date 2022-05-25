using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class LivePlayerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Ping { get; set; }
        public int Num { get; set; }
        public int Rate { get; set; }
        public string Team { get; set; } = string.Empty;
        public TimeSpan Time { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public string CountryCode { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        public Guid? GameServerServerId { get; set; }
    }
}
