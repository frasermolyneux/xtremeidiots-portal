using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// View model supplying an admin action with associated player data (fetched separately when not embedded).
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="MyAdminActionDetailsViewModel"/>.
/// </remarks>
/// <param name="adminAction">The admin action DTO.</param>
/// <param name="player">The player DTO (fetched independently if not present on the admin action).</param>
public class MyAdminActionDetailsViewModel(AdminActionDto adminAction, PlayerDto? player)
{

    /// <summary>
    /// Gets the admin action.
    /// </summary>
    public AdminActionDto AdminAction { get; } = adminAction ?? throw new ArgumentNullException(nameof(adminAction));

    /// <summary>
    /// Gets the player related to the admin action (may be null if not found).
    /// </summary>
    public PlayerDto? Player { get; } = player;
}
