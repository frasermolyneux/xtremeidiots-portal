using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class CreateUserProfileDto
    {
        public CreateUserProfileDto(string xtremeIdiotsForumId, string displayName, string email)
        {
            XtremeIdiotsForumId = xtremeIdiotsForumId;
            DisplayName = displayName;
            Email = email;
        }

        [JsonProperty]
        public string? IdentityOid { get; set; }

        [JsonProperty]
        public string? XtremeIdiotsForumId { get; private set; }

        [JsonProperty]
        public string? DisplayName { get; private set; }

        [JsonProperty]
        public string? Title { get; set; }

        [JsonProperty]
        public string? FormattedName { get; set; }

        [JsonProperty]
        public string? PrimaryGroup { get; set; }

        [JsonProperty]
        public string? Email { get; private set; }

        [JsonProperty]
        public string? PhotoUrl { get; set; }

        [JsonProperty]
        public string? ProfileUrl { get; set; }

        [JsonProperty]
        public string? TimeZone { get; set; }
    }
}
