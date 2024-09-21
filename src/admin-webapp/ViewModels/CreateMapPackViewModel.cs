using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class CreateMapPackViewModel
    {
        public Guid GameServerId { get; set; }


        [Required][DisplayName("Title")] public string Title { get; set; }

        [Required][DisplayName("Description")] public string Description { get; set; }

        [DisplayName("Game Mode")] public string GameMode { get; set; }

        [DisplayName("SyncToGameServer")] public bool SyncToGameServer { get; set; }
    }
}
