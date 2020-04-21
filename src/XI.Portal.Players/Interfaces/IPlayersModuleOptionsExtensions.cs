using System;
using XI.Portal.Players.Configuration;

namespace XI.Portal.Players.Interfaces
{
    public static class PlayersModuleOptionsExtensions
    {
        public static void ConfigurePlayersRepository(this IPlayersModuleOptions options, Action<IPlayersRepositoryOptions> repositoryOptions)
        {
            options.PlayersRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureAdminActionsRepository(this IPlayersModuleOptions options, Action<IAdminActionsRepositoryOptions> repositoryOptions)
        {
            options.AdminActionsRepositoryOptions = repositoryOptions;
        }
    }
}