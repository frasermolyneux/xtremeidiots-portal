using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for creating a new map pack
/// </summary>
/// <remarks>
/// Represents the form data required to create a map pack for a specific game server
/// </remarks>
public class CreateMapPackViewModel
{
    /// <summary>
    /// Gets or sets the ID of the game server this map pack belongs to
    /// </summary>
    public Guid GameServerId { get; set; }

    /// <summary>
    /// Gets or sets the title of the map pack
    /// </summary>
    [Required]
    [DisplayName("Title")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the map pack
    /// </summary>
    [Required]
    [DisplayName("Description")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the game mode for this map pack
    /// </summary>
    [DisplayName("Game Mode")]
    public required string GameMode { get; set; }

    /// <summary>
    /// Gets or sets whether this map pack should be synchronized to the game server
    /// </summary>
    [DisplayName("SyncToGameServer")]
    public bool SyncToGameServer { get; set; }
}