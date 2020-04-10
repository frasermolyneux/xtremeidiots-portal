using System;
using XI.Portal.Players.Configuration;

namespace XI.Portal.Players.Extensions
{
    public static class PlayersModuleOptionsExtensions
    {
        public static void ConfigurePlayersRepository(this IPlayersModuleOptions options, Action<IPlayersRepositoryOptions> repositoryOptions)
        {
            options.PlayersRepositoryOptions = repositoryOptions;
        }
    }
}