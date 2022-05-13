using System;
using XI.Portal.Web.Configuration;

namespace XI.Portal.Web.Extensions
{
    public static class UsersModuleOptionsExtensions
    {
        public static void ConfigureUsersRepository(this IUsersModuleOptions options, Action<IUsersRepositoryOptions> repositoryOptions)
        {
            options.UsersRepositoryOptions = repositoryOptions;
        }
    }
}