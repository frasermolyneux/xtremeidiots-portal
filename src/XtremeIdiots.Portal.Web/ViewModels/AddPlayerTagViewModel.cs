using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;

namespace XtremeIdiots.Portal.Web.ViewModels;

public class AddPlayerTagViewModel
{
    [Required]
    public Guid PlayerId { get; set; }

    [Required]
    [Display(Name = "Tag")]
    public Guid TagId { get; set; }

    public PlayerDto? Player { get; set; }

    public List<TagDto> AvailableTags { get; set; } = [];
}