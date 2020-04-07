using System;
using System.ComponentModel;

namespace XI.Portal.Data.Legacy.Models
{
    public class FileMonitors
    {
        public Guid FileMonitorId { get; set; }
        [DisplayName("File Path")]
        public string FilePath { get; set; }
        public long BytesRead { get; set; }
        [DisplayName("Last Read")]
        public DateTime LastRead { get; set; }
        public string LastError { get; set; }
        public Guid? GameServerServerId { get; set; }

        [DisplayName("Server")]
        public virtual GameServers GameServerServer { get; set; }
    }
}