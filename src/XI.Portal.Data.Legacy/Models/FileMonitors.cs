using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace XI.Portal.Data.Legacy.Models
{
    public class FileMonitors
    {
        public Guid FileMonitorId { get; set; }

        [Required] [DisplayName("File Path")] public string FilePath { get; set; }

        [DisplayName("Bytes Read")] public long BytesRead { get; set; }

        [DisplayName("Last Read")] public DateTime LastRead { get; set; }

        [Obsolete] public string LastError { get; set; }

        [DisplayName("Server")] public Guid GameServerServerId { get; set; }

        [DisplayName("Server")] public virtual GameServers GameServerServer { get; set; }
    }
}