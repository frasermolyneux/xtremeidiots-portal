using System;

namespace XI.Portal.Servers.Interfaces
{
    public interface IServersModuleOptions
    {
        Action<IGameServersRepositoryOptions> GameServersRepositoryOptions { get; set; }
        Action<IBanFileMonitorsRepositoryOptions> BanFileMonitorsRepositoryOptions { get; set; }
        Action<IFileMonitorsRepositoryOptions> FileMonitorsRepositoryOptions { get; set; }
        Action<IRconMonitorsRepositoryOptions> RconMonitorsRepositoryOptions { get; set; }
        Action<IGameServerStatusRepositoryOptions> GameServerStatusRepositoryOptions { get; set; }
        Action<IChatLogsRepositoryOptions> ChatLogsRepositoryOptions { get; set; }

        void Validate();
    }
}