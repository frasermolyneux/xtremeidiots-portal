﻿using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.UserProfiles;

namespace XtremeIdiots.Portal.AdminWebApp.Models
{
    public class UserProfileViewModel
    {
        public UserProfileViewModel(UserProfileDto userProfile)
        {
            UserProfile = userProfile;
        }

        public UserProfileDto UserProfile { get; private set; }
    }
}