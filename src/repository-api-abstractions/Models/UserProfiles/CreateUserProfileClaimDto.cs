using Newtonsoft.Json;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles
{
    public class CreateUserProfileClaimDto
    {
        public CreateUserProfileClaimDto(Guid userProfileId, string claimType, string claimValue, bool systemGenerated)
        {
            UserProfileId = userProfileId;
            ClaimType = claimType;
            ClaimValue = claimValue;
            SystemGenerated = systemGenerated;
        }

        [JsonProperty]
        public Guid UserProfileId { get; private set; }

        [JsonProperty]
        public bool SystemGenerated { get; private set; }

        [JsonProperty]
        public string ClaimType { get; private set; }

        [JsonProperty]
        public string ClaimValue { get; private set; }
    }
}
