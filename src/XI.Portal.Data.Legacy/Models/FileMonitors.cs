using System;

namespace XI.Portal.Data.Legacy.Models
{
    public class FileMonitors
    {
        public Guid FileMonitorId { get; set; }
        public string FilePath { get; set; }
        public long BytesRead { get; set; }
        public DateTime LastRead { get; set; }
        public string LastError { get; set; }
        public Guid? GameServerServerId { get; set; }

        public virtual GameServers GameServerServer { get; set; }
    }
}