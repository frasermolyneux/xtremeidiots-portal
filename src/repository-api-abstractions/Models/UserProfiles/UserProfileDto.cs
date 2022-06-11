using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class UserProfileDto
    {
        [JsonProperty]
        public Guid Id { get; internal set; }

        [JsonProperty]
        public string IdentityOid { get; internal set; } = string.Empty;

        [JsonProperty]
        public string XtremeIdiotsForumId { get; internal set; } = string.Empty;

        [JsonProperty]
        public string DisplayName { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Title { get; internal set; } = string.Empty;

        [JsonProperty]
        public string FormattedName { get; internal set; } = string.Empty;

        [JsonProperty]
        public string PrimaryGroup { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Email { get; internal set; } = string.Empty;

        [JsonProperty]
        public string PhotoUrl { get; internal set; } = string.Empty;

        [JsonProperty]
        public string ProfileUrl { get; internal set; } = string.Empty;

        [JsonProperty]
        public string TimeZone { get; internal set; } = string.Empty;

        [JsonProperty]
        public List<UserProfileClaimDto> UserProfileClaimDtos { get; set; } = new List<UserProfileClaimDto>();
    }
}
