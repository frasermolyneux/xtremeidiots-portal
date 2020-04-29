using System;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Configuration
{
    internal class ServersModuleOptions : IServersModuleOptions
    {
        public Action<IFileMonitorsRepositoryOptions> FileMonitorsRepositoryOptions { get; set; }
        public Action<IRconMonitorsRepositoryOptions> RconMonitorsRepositoryOptions { get; set; }
        public Action<IGameServerStatusRepositoryOptions> GameServerStatusRepositoryOptions { get; set; }
        public Action<IChatLogsRepositoryOptions> ChatLogsRepositoryOptions { get; set; }

        public void Validate()
        {
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