using System;

namespace XI.Portal.Users.Configuration
{
    public interface IUsersModuleOptions
    {
        Action<IUsersRepositoryOptions> UsersRepositoryOptions { get; set; }
    }
}