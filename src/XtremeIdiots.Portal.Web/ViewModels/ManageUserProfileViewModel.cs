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
}

/// <summary>
/// Lightweight projection of ASP.NET Identity user properties exposed to UI.
/// </summary>
public class IdentityUserSummary
{
    public string Id { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
}
