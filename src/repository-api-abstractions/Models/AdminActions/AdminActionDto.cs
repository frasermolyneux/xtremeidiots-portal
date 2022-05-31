using System.Text.Json.Serialization;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions
{
    public class AdminActionDto
    {
        public Guid AdminActionId { get; set; }
        public Guid PlayerId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType GameType { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Guid { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AdminActionType Type { get; set; }
        public string Text { get; set; } = string.Empty;

        public DateTime? Expires { get; set; }
        public int ForumTopicId { get; set; }
        public DateTime Created { get; set; }

        public string? AdminId { get; set; }
        public string? AdminName { get; set; }
    }
}
