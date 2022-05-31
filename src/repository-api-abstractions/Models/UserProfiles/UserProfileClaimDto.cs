using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class UserProfileClaimDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public Guid UserProfileId { get; set; }

        [JsonProperty]
        public bool SystemGenerated { get; set; }

        [JsonProperty]
        public string? ClaimType { get; set; }

        [JsonProperty]
        public string? ClaimValue { get; set; }
    }
}
