using System;
using XI.Portal.Users.Configuration;

namespace XI.Portal.Users.Extensions
{
    public static class UsersModuleOptionsExtensions
    {
        public static void ConfigureUsersRepository(this IUsersModuleOptions options, Action<IUsersRepositoryOptions> repositoryOptions)
        {
            options.UsersRepositoryOptions = repositoryOptions;
        }
    }
}