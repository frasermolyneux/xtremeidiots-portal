using Newtonsoft.Json;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports
{
    public class ReportDto
    {
        [JsonProperty]
        public Guid Id { get; internal set; }
        [JsonProperty]
        public Guid? PlayerId { get; internal set; }
        [JsonProperty]
        public Guid? UserProfileId { get; internal set; }
        [JsonProperty]
        public Guid? ServerId { get; internal set; }
        [JsonProperty]
        public GameType GameType { get; internal set; }
        [JsonProperty]
        public string? Comments { get; internal set; }
        [JsonProperty]
        public DateTime Timestamp { get; internal set; }
        [JsonProperty]
        public Guid? AdminUserProfileId { get; internal set; }
        [JsonProperty]
        public string? AdminClosingComments { get; internal set; }
        [JsonProperty]
        public bool Closed { get; internal set; }
        [JsonProperty]
        public DateTime? ClosedTimestamp { get; internal set; }

        [JsonProperty]
        public UserProfileDto? UserProfile { get; internal set; }
        [JsonProperty]
        public UserProfileDto? AdminUserProfile { get; internal set; }
    }
}
