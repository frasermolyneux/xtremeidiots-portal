using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class UserProfileClaimExtensions
    {
        public static UserProfileClaimDto ToDto(this UserProfileClaim userProfileClaim)
        {
            var userProfileClaimDto = new UserProfileClaimDto
            {
                Id = userProfileClaim.Id,
                UserProfileId = userProfileClaim.UserProfileId,
                SystemGenerated = userProfileClaim.SystemGenerated,
                ClaimType = userProfileClaim.ClaimType,
                ClaimValue = userProfileClaim.ClaimValue
            };

            return userProfileClaimDto;
        }
    }
}
