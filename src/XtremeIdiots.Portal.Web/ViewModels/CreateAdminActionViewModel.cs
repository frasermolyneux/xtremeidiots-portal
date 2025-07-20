using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model for creating new admin actions against players
/// </summary>
public class CreateAdminActionViewModel
{
    /// <summary>
    /// Gets or sets the ID of the player this admin action applies to
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the type of admin action (kick, ban, etc.)
    /// </summary>
    public AdminActionType Type { get; set; }

    /// <summary>
    /// Gets or sets the reason for the admin action
    /// </summary>
    [Required]
    [DisplayName("Reason")]
    [MinLength(3, ErrorMessage = "You must enter a reason for the admin action")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the admin ID who is creating this action
    /// </summary>
    public string? AdminId { get; set; }

    /// <summary>
    /// Gets or sets when this admin action expires (for temporary bans)
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Gets or sets the player data for display purposes
    /// </summary>
    public PlayerDto? PlayerDto { get; set; }
}