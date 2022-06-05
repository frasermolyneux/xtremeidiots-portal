using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions
{
    public class AdminActionDto
    {
        [JsonProperty]
        public Guid AdminActionId { get; set; }

        [JsonProperty]
        public Guid PlayerId { get; set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }

        [JsonProperty]
        public string Username { get; set; } = string.Empty;

        [JsonProperty]
        public string Guid { get; set; } = string.Empty;

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public AdminActionType Type { get; set; }

        [JsonProperty]
        public string Text { get; set; } = string.Empty;

        [JsonProperty]
        public DateTime? Expires { get; set; }

        [JsonProperty]
        public int ForumTopicId { get; set; }

        [JsonProperty]
        public DateTime Created { get; set; }

        [JsonProperty]
        public string? AdminId { get; set; }

        [JsonProperty]
        public string? AdminName { get; set; }
    }
}
