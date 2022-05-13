using System;

namespace XI.Portal.Web.Configuration
{
    internal class UsersModuleOptions : IUsersModuleOptions
    {
        public Action<IUsersRepositoryOptions> UsersRepositoryOptions { get; set; }
    }
}