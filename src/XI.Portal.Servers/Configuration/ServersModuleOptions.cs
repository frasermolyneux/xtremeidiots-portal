using System;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Configuration
{
    internal class ServersModuleOptions : IServersModuleOptions
    {
        public Action<IGameServersRepositoryOptions> GameServersRepositoryOptions { get; set; }
        public Action<IBanFileMonitorsRepositoryOptions> BanFileMonitorsRepositoryOptions { get; set; }
        public Action<IFileMonitorsRepositoryOptions> FileMonitorsRepositoryOptions { get; set; }
        public Action<IRconMonitorsRepositoryOptions> RconMonitorsRepositoryOptions { get; set; }
        public Action<IGameServerStatusRepositoryOptions> GameServerStatusRepositoryOptions { get; set; }
        public Action<IChatLogsRepositoryOptions> ChatLogsRepositoryOptions { get; set; }

        public void Validate()
        {
            if (GameServersRepositoryOptions == null)
                throw new NullReferenceException(nameof(GameServersRepositoryOptions));

            if (BanFileMonitorsRepositoryOptions == null)
                throw new NullReferenceException(nameof(BanFileMonitorsRepositoryOptions));

            if (FileMonitorsRepositoryOptions == null)
                throw new NullReferenceException(nameof(FileMonitorsRepositoryOptions));

            if (RconMonitorsRepositoryOptions == null)
                throw new NullReferenceException(nameof(RconMonitorsRepositoryOptions));

            if (GameServerStatusRepositoryOptions == null)
                throw new NullReferenceException(nameof(GameServerStatusRepositoryOptions));

            if (ChatLogsRepositoryOptions == null)
                throw new NullReferenceException(nameof(ChatLogsRepositoryOptions));
        }
    }
}