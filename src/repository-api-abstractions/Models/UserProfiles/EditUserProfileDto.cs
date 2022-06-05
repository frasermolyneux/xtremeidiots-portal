using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class EditUserProfileDto
    {
        public EditUserProfileDto(Guid id)
        {
            Id = id;
        }

        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public string? IdentityOid { get; set; }

        [JsonProperty]
        public string? XtremeIdiotsForumId { get; set; }

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
