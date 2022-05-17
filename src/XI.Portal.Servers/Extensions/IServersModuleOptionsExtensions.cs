using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Extensions
{
    public static class ServersModuleOptionsExtensions
    {
        public static void ConfigureGameServerStatusRepository(this IServersModuleOptions options, Action<IGameServerStatusRepositoryOptions> repositoryOptions)
        {
            options.GameServerStatusRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureGameServerStatusStatsRepository(this IServersModuleOptions options, Action<IGameServerStatusStatsRepositoryOptions> repositoryOptions)
        {
            options.GameServerStatusStatsRepositoryOptions = repositoryOptions;
        }
    }
}