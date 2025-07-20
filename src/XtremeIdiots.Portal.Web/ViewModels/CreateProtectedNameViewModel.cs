using System.ComponentModel.DataAnnotations;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for creating a new protected name for a player
/// </summary>
public class CreateProtectedNameViewModel
{
    /// <summary>
    /// Initializes a new instance of the CreateProtectedNameViewModel class
    /// </summary>
    public CreateProtectedNameViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the CreateProtectedNameViewModel class with a player ID
    /// </summary>
    /// <param name="playerId">The ID of the player to create a protected name for</param>
    public CreateProtectedNameViewModel(Guid playerId)
    {
        PlayerId = playerId;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the player
    /// </summary>
    [Required]
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the protected name for the player
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Protected Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player information for display purposes
    /// </summary>
    public PlayerDto? Player { get; set; }
}