using System;

namespace XI.Portal.Servers.Interfaces
{
    public interface IServersModuleOptions
    {
        Action<IGameServerStatusRepositoryOptions> GameServerStatusRepositoryOptions { get; set; }
        Action<IGameServerStatusStatsRepositoryOptions> GameServerStatusStatsRepositoryOptions { get; set; }
        Action<ILogFileMonitorStateRepositoryOptions> LogFileMonitorStateRepositoryOptions { get; set; }
    }
}