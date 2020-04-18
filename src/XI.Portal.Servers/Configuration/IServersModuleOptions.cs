using System;

namespace XI.Portal.Servers.Configuration
{
    public interface IServersModuleOptions
    {
        Action<IGameServersRepositoryOptions> GameServersRepositoryOptions { get; set; }
        Action<IBanFileMonitorsRepositoryOptions> BanFileMonitorsRepositoryOptions { get; set; }
        Action<IFileMonitorsRepositoryOptions> FileMonitorsRepositoryOptions { get; set; }
        Action<IRconMonitorsRepositoryOptions> RconMonitorsRepositoryOptions { get; set; }
        Action<IGameServerStatusRepositoryOptions> GameServerStatusRepositoryOptions { get; set; }

        void Validate();
    }
}