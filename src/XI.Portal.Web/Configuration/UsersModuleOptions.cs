using System;

namespace XI.Portal.Users.Configuration
{
    internal class UsersModuleOptions : IUsersModuleOptions
    {
        public Action<IUsersRepositoryOptions> UsersRepositoryOptions { get; set; }
    }
}