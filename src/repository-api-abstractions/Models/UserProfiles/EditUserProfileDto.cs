using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class EditUserProfileDto
    {
        public EditUserProfileDto(Guid userProfileId)
        {
            UserProfileId = userProfileId;
        }

        [JsonProperty]
        public Guid UserProfileId { get; private set; }

        [JsonProperty]
        public string? IdentityOid { get; set; }

        [JsonProperty]
        public string? XtremeIdiotsForumId { get; set; }

        [JsonProperty]
        public string? DemoAuthKey { get; set; }

        [JsonProperty]
        public string? DisplayName { get; set; }

        [JsonProperty]
        public string? Title { get; set; }

        [JsonProperty]
        public string? FormattedName { get; set; }

        [JsonProperty]
        public string? PrimaryGroup { get; set; }

        [JsonProperty]
        public string? Email { get; set; }

        [JsonProperty]
        public string? PhotoUrl { get; set; }

        [JsonProperty]
        public string? ProfileUrl { get; set; }

        [JsonProperty]
        public string? TimeZone { get; set; }
    }
}
