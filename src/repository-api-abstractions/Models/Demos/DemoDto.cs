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
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType Game { get; internal set; }

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public string FileName { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime? Date { get; internal set; }

        [JsonProperty]
        public string Map { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Mod { get; internal set; } = string.Empty;

        [JsonProperty]
        public string GameType { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Server { get; internal set; } = string.Empty;

        [JsonProperty]
        public long Size { get; internal set; }

        [JsonProperty]
        public string UserId { get; internal set; } = string.Empty;

        [JsonProperty]
        public string UploadedBy { get; internal set; } = string.Empty;

        [JsonProperty]
        public string DemoFileUri { get; internal set; } = string.Empty;

        [JsonProperty]
        public UserProfileDto? UserProfileDto { get; internal set; }
    }
}