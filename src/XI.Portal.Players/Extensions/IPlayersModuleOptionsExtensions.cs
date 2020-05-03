using System;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Extensions
{
    public static class PlayersModuleOptionsExtensions
    {
        public static void ConfigurePlayerLocationsRepository(this IPlayersModuleOptions options, Action<IPlayerLocationsRepositoryOptions> repositoryOptions)
        {
            options.PlayerLocationsRepositoryOptions = repositoryOptions;
        }
    }
}