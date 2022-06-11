using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Maps
{
    public class MapDto
    {
        [JsonProperty]
        public Guid MapId { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; internal set; }

        [JsonProperty]
        public string MapName { get; internal set; } = string.Empty;

        [JsonProperty]
        public string MapImageUri { get; internal set; } = string.Empty;

        [JsonProperty]
        public int TotalLikes { get; internal set; } = 0;

        [JsonProperty]
        public int TotalDislikes { get; internal set; } = 0;

        [JsonProperty]
        public int TotalVotes { get; internal set; } = 0;

        [JsonProperty]
        public double LikePercentage { get; internal set; } = 0;

        [JsonProperty]
        public double DislikePercentage { get; internal set; } = 0;

        [JsonProperty]
        public List<MapFileDto> MapFiles { get; internal set; } = new List<MapFileDto>();
    }
}