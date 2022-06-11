using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class UserProfileClaimDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Guid UserProfileId { get; internal set; }

        [JsonProperty]
        public bool SystemGenerated { get; internal set; }

        [JsonProperty]
        public string ClaimType { get; internal set; } = string.Empty;

        [JsonProperty]
        public string ClaimValue { get; internal set; } = string.Empty;
    }
}
