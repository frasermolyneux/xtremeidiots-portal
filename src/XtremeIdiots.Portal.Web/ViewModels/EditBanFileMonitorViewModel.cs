using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class EditBanFileMonitorViewModel
    {
        public Guid BanFileMonitorId { get; set; }

        [Required][DisplayName("File Path")] public string FilePath { get; set; }

        [DisplayName("Remote File Size")] public long? RemoteFileSize { get; set; }

        [DisplayName("Last Read")] public DateTime? LastSync { get; set; }

        [DisplayName("Server")] public Guid GameServerId { get; set; }

        [DisplayName("Server")] public GameServerDto? GameServer { get; set; }
    }
}
