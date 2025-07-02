using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;

namespace XtremeIdiots.Portal.Web.ViewModels
{
    public class AddPlayerTagViewModel
    {
        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        [Display(Name = "Tag")]
        public Guid TagId { get; set; }

        public PlayerDto? Player { get; set; }

        public List<TagDto> AvailableTags { get; set; } = new List<TagDto>();
    }
}
