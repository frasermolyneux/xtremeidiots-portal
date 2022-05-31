using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.AdminWebApp.Models
{
    public class UserProfileViewModel
    {
        public UserProfileDto UserProfile { get; set; }
        public List<UserProfileClaimDto> UserProfileClaims { get; set; } = new List<UserProfileClaimDto>();
    }
}