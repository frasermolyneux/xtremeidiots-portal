using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions
{
    public class AdminActionDto
    {
        [JsonProperty]
        public Guid AdminActionId { get; internal set; }

        [JsonProperty]
        public Guid PlayerId { get; internal set; }

        [JsonProperty]
        public Guid? UserProfileId { get; internal set; }

        [JsonProperty]
        public int? ForumTopicId { get; internal set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public AdminActionType Type { get; internal set; }

        [JsonProperty]
        public string Text { get; internal set; } = string.Empty;

        [JsonProperty]
        public DateTime Created { get; internal set; }

        [JsonProperty]
        public DateTime? Expires { get; internal set; }

        [JsonProperty]
        public PlayerDto? PlayerDto { get; internal set; }

        [JsonProperty]
        public UserProfileDto? UserProfileDto { get; internal set; }
    }
}
