using Newtonsoft.Json;

using System.Text.Json.Serialization;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions
{
    public class CreateAdminActionDto
    {
        public CreateAdminActionDto(Guid playerId, AdminActionType type, string text)
        {
            PlayerId = playerId;
            Type = type;
            Text = text;
        }

        [JsonProperty]
        public Guid PlayerId { get; private set; }

        [JsonProperty]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        public AdminActionType Type { get; private set; }

        [JsonProperty]
        public string Text { get; private set; }

        [JsonProperty]
        public DateTime? Expires { get; set; }

        [JsonProperty]
        public int? ForumTopicId { get; set; }

        [JsonProperty]
        public string? AdminId { get; set; }
    }
}
