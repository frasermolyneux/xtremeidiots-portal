using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions
{
    public class EditAdminActionDto
    {
        public EditAdminActionDto(Guid adminActionId)
        {
            AdminActionId = adminActionId;
        }

        [JsonProperty]
        public Guid AdminActionId { get; private set; }

        [JsonProperty]
        public string? Text { get; set; }

        [JsonProperty]
        public DateTime? Expires { get; set; }

        [JsonProperty]
        public int? ForumTopicId { get; set; }

        [JsonProperty]
        public string? AdminId { get; set; }
    }
}
