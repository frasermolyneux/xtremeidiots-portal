using System;

namespace XI.Portal.Users.Configuration
{
    internal class UsersModuleOptions : IUsersModuleOptions
    {
        public Action<IUsersRepositoryOptions> UsersRepositoryOptions { get; set; }

        public void Validate()
        {
            if (UsersRepositoryOptions == null)
                throw new NullReferenceException(nameof(UsersRepositoryOptions));
        }
    }
}