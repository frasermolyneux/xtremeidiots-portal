using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class BanFileMonitors
    {
        public Guid BanFileMonitorId { get; set; }
        public string FilePath { get; set; }
        public long RemoteFileSize { get; set; }
        public DateTime LastSync { get; set; }
        [Obsolete] public string LastError { get; set; }
        public Guid? GameServerServerId { get; set; }
        public virtual GameServers GameServerServer { get; set; }
    }
}