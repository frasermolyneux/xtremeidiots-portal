using Newtonsoft.Json;
using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class MapDto
    {
        [JsonProperty]
        public Guid MapId { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public string MapName { get; set; } = string.Empty;

        [JsonProperty]
        public string MapImageUri { get; set; } = string.Empty;

        [JsonProperty]
        public int TotalLikes { get; set; } = 0;

        [JsonProperty]
        public int TotalDislikes { get; set; } = 0;

        [JsonProperty]
        public int TotalVotes { get; set; } = 0;

        [JsonProperty]
        public double LikePercentage { get; set; } = 0;

        [JsonProperty]
        public double DislikePercentage { get; set; } = 0;

        [JsonProperty]
        public List<MapFileDto> MapFiles { get; set; } = new List<MapFileDto>();
    }
}