using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Demos
{
    public class DemoDto
    {
        [JsonProperty]
        public Guid DemoId { get; internal set; }

        [JsonProperty]
        public Guid UserProfileId { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; internal set; }

        [JsonProperty]
        public string Title { get; internal set; } = string.Empty;

        [JsonProperty]
        public string FileName { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime? Created { get; internal set; }

        [JsonProperty]
        public string Map { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Mod { get; internal set; } = string.Empty;

        [JsonProperty]
        public string GameMode { get; internal set; } = string.Empty;

        [JsonProperty]
        public string ServerName { get; internal set; } = string.Empty;

        [JsonProperty]
        public long FileSize { get; internal set; }

        [JsonProperty]
        public string FileUri { get; internal set; } = string.Empty;


        [JsonProperty]
        public UserProfileDto? UserProfile { get; internal set; }
    }
}