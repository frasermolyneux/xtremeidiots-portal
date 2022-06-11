using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions
{
    public class AdminActionDto
    {
        [JsonProperty]
        public Guid AdminActionId { get; internal set; }

        [JsonProperty]
        public Guid PlayerId { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; internal set; }

        [JsonProperty]
        public string Username { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Guid { get; internal set; } = string.Empty;

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public AdminActionType Type { get; internal set; }

        [JsonProperty]
        public string Text { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime? Expires { get; internal set; }

        [JsonProperty]
        public int ForumTopicId { get; internal set; }

        [JsonProperty]
        public DateTime Created { get; internal set; }

        [JsonProperty]
        public string? AdminId { get; internal set; }

        [JsonProperty]
        public string? AdminName { get; internal set; }
    }
}
