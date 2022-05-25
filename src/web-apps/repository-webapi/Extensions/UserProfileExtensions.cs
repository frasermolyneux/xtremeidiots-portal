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
                XtremeIdiotsForumId = userProfile.XtremeIdiotsForumId,
                DisplayName = userProfile.DisplayName,
                Title = userProfile.Title,
                FormattedName = userProfile.FormattedName,
                PrimaryGroup = userProfile.PrimaryGroup,
                Email = userProfile.Email,
                PhotoUrl = userProfile.PhotoUrl,
                ProfileUrl = userProfile.ProfileUrl,
                TimeZone = userProfile.TimeZone
            };

            return userProfileDto;
        }
    }
}
