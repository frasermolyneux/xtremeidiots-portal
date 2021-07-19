using System;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Servers.Configuration
{
    internal class ServersModuleOptions : IServersModuleOptions
    {
        public Action<IGameServerStatusRepositoryOptions> GameServerStatusRepositoryOptions { get; set; }
        public Action<IGameServerStatusStatsRepositoryOptions> GameServerStatusStatsRepositoryOptions { get; set; }
    }
}