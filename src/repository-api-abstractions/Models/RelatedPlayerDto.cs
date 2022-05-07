using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class RelatedPlayerDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        public string Username { get; set; } = string.Empty;
        public Guid PlayerId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
}