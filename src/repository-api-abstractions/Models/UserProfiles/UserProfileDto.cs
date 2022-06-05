using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class UserProfileDto
    {
        [JsonProperty]
        public Guid Id { get; internal set; }

        [JsonProperty]
        public string? IdentityOid { get; internal set; }

        [JsonProperty]
        public string? XtremeIdiotsForumId { get; internal set; }

        [JsonProperty]
        public string? DisplayName { get; internal set; }

        [JsonProperty]
        public string? Title { get; internal set; }

        [JsonProperty]
        public string? FormattedName { get; internal set; }

        [JsonProperty]
        public string? PrimaryGroup { get; internal set; }

        [JsonProperty]
        public string? Email { get; internal set; }

        [JsonProperty]
        public string? PhotoUrl { get; internal set; }

        [JsonProperty]
        public string? ProfileUrl { get; internal set; }

        [JsonProperty]
        public string? TimeZone { get; internal set; }

        [JsonProperty]
        public List<UserProfileClaimDto> UserProfileClaimDtos { get; set; } = new List<UserProfileClaimDto>();
    }
}
