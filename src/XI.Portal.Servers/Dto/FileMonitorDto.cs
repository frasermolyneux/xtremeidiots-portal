using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace XI.Portal.Servers.Dto
{
    public class FileMonitorDto
    {
        public Guid FileMonitorId { get; set; }
        [Required] [DisplayName("File Path")] public string FilePath { get; set; }
        [DisplayName("Bytes Read")] public long BytesRead { get; set; }
        [DisplayName("Last Read")] public DateTime LastRead { get; set; }
        [DisplayName("Server")] public Guid ServerId { get; set; }
        [DisplayName("Server")] public GameServerDto GameServer { get; set; }
    }
}