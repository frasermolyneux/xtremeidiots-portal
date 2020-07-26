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

        public static void ConfigurePlayersCacheRepository(this IPlayersModuleOptions options, Action<IPlayersCacheRepositoryOptions> repositoryOptions)
        {
            options.PlayersCacheRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureExternalBansRepositoryOptions(this IPlayersModuleOptions options, Action<IExternalBansRepositoryOptions> repositoryOptions)
        {
            options.ExternalBansRepositoryOptions = repositoryOptions;
        }
    }
}