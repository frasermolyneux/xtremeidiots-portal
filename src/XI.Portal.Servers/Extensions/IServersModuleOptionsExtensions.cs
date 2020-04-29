using System;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Extensions
{
    public static class ServersModuleOptionsExtensions
    {
        public static void ConfigureFileMonitorsRepository(this IServersModuleOptions options, Action<IFileMonitorsRepositoryOptions> repositoryOptions)
        {
            options.FileMonitorsRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureRconMonitorsRepository(this IServersModuleOptions options, Action<IRconMonitorsRepositoryOptions> repositoryOptions)
        {
            options.RconMonitorsRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureGameServerStatusRepository(this IServersModuleOptions options, Action<IGameServerStatusRepositoryOptions> repositoryOptions)
        {
            options.GameServerStatusRepositoryOptions = repositoryOptions;
        }

        public static void ConfigureChatLogsRepository(this IServersModuleOptions options, Action<IChatLogsRepositoryOptions> repositoryOptions)
        {
            options.ChatLogsRepositoryOptions = repositoryOptions;
        }
    }
}