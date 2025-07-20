using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for editing an existing admin action
/// </summary>
public class EditAdminActionViewModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the admin action
    /// </summary>
    public Guid AdminActionId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the player
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the type of admin action
    /// </summary>
    public AdminActionType Type { get; set; }

    /// <summary>
    /// Gets or sets the reason text for the admin action
    /// </summary>
    [Required]
    [DisplayName("Reason")]
    [MinLength(3, ErrorMessage = "You must enter a reason for the admin action")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the administrator who performed the action
    /// </summary>
    public string? AdminId { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time for the admin action
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Gets or sets the player information for display purposes
    /// </summary>
    public PlayerDto? PlayerDto { get; set; }
}