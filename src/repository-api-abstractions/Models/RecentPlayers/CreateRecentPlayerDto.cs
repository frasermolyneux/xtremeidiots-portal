using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.RecentPlayers
{
    public class CreateRecentPlayerDto
    {
        public CreateRecentPlayerDto(string name, GameType gameType, Guid playerId)
        {
            Name = name;
            GameType = gameType;
            PlayerId = playerId;
        }

        [JsonProperty]
        public string Name { get; private set; }

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
        public GameType GameType { get; private set; }

        [JsonProperty]
        public Guid PlayerId { get; private set; }

        [JsonProperty]
        public Guid? ServerId { get; set; }
    }
}
