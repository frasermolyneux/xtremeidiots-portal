using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace XI.Portal.Servers.Dto
{
    public class BanFileMonitorDto
    {
        public Guid BanFileMonitorId { get; set; }

        [Required] [DisplayName("File Path")] public string FilePath { get; set; }

        [DisplayName("Remote File Size")] public long RemoteFileSize { get; set; }

        [DisplayName("Last Read")] public DateTime LastSync { get; set; }

        [DisplayName("Server")] public Guid ServerId { get; set; }

        [DisplayName("Server")] public GameServerDto GameServer { get; set; }
    }
}