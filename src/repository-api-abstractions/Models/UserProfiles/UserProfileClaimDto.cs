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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string ClaimType { get; set; }

        [JsonProperty]
        public string ClaimValue { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
