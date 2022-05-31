using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players
{
    public class RelatedPlayerDto
    {
        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public string Username { get; set; } = string.Empty;

        [JsonProperty]
        public Guid PlayerId { get; set; }

        [JsonProperty]
        public string IpAddress { get; set; } = string.Empty;
    }
}