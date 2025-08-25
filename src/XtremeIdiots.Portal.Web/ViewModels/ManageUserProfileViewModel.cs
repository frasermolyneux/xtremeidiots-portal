using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.UserProfiles;

namespace XtremeIdiots.Portal.Web.ViewModels;

/// <summary>
/// Composite view model combining repository user profile data with ASP.NET Identity user information.
/// </summary>
public class ManageUserProfileViewModel
{
    /// <summary>
    /// The repository user profile DTO.
    /// </summary>
    public UserProfileDto Profile { get; set; } = null!;

    /// <summary>
    /// Identity data for the associated user (may be partial / null if user not found in Identity store).
    /// </summary>
    public IdentityUserSummary? Identity { get; set; }
        = null;
}

/// <summary>
/// Lightweight projection of ASP.NET Identity user properties exposed to UI.
/// </summary>
public class IdentityUserSummary
{
    public string Id { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
        = false;
    public bool LockoutEnabled { get; set; }
        = false;
    public DateTimeOffset? LockoutEnd { get; set; }
        = null;
    public int AccessFailedCount { get; set; }
        = 0;
    public bool TwoFactorEnabled { get; set; }
        = false;
    public string? PhoneNumber { get; set; }
        = null;
    public bool PhoneNumberConfirmed { get; set; }
        = false;
}
