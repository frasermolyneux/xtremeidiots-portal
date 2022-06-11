using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class AdminActionViewModel
    {
        public Guid AdminActionId { get; set; }
        public Guid PlayerId { get; set; }
        public AdminActionType Type { get; set; }

        [Required]
        [DisplayName("Reason")]
        [MinLength(3, ErrorMessage = "You must enter a reason for the admin action")]
        public string Text { get; set; } = string.Empty;

        public string? AdminId { get; set; }

        public DateTime? Expires { get; set; }

        public PlayerDto? PlayerDto { get; set; }
    }
}
