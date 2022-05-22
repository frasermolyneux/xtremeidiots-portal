using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class UserProfileExtensions
    {
        public static UserProfileDto ToDto(this UserProfile userProfile)
        {
            var userProfileDto = new UserProfileDto
            {
                Id = userProfile.Id,
                IdentityOid = userProfile.IdentityOid,
                XtremeIdiotsForumId = userProfile.XtremeIdiotsForumId
            };

            return userProfileDto;
        }
    }
}
