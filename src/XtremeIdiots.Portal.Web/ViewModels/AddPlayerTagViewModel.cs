using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for adding a tag to a player
/// </summary>
public class AddPlayerTagViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the player to add the tag to
    /// </summary>
    [Required]
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the tag to add to the player
    /// </summary>
    [Required]
    [Display(Name = "Tag")]
    public Guid TagId { get; set; }

    /// <summary>
    /// Gets or sets the player data for display purposes
    /// </summary>
    public PlayerDto? Player { get; set; }

    /// <summary>
    /// Gets or sets the list of available tags that can be assigned to the player
    /// </summary>
    public List<TagDto> AvailableTags { get; set; } = [];
}