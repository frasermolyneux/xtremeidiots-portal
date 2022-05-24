using XI.Portal.Players.Interfaces;

namespace XI.Portal.Players.Extensions
{
    public static class PlayersModuleOptionsExtensions
    {
        public static void ConfigureBanFilesRepositoryOptions(this IPlayersModuleOptions options, Action<IBanFilesRepositoryOptions> repositoryOptions)
        {
            options.BanFilesRepositoryOptions = repositoryOptions;
        }
    }
}