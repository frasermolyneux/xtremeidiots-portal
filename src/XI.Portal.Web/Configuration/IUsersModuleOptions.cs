using System;

namespace XI.Portal.Web.Configuration
{
    public interface IUsersModuleOptions
    {
        Action<IUsersRepositoryOptions> UsersRepositoryOptions { get; set; }
    }
}