using System.ComponentModel.DataAnnotations;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class CreateProtectedNameViewModel
    {
        public CreateProtectedNameViewModel()
        {
        }

        public CreateProtectedNameViewModel(Guid playerId)
        {
            PlayerId = playerId;
        }

        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Protected Name")]
        public string Name { get; set; } = string.Empty;

        public PlayerDto? Player { get; set; }
    }
}