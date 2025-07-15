using Microsoft.AspNetCore.Authorization;

namespace XtremeIdiots.Portal.Web.Auth.Requirements
{
    /// <summary>
    /// Authorization requirement for accessing user profile management functionality.
    /// This requirement enables authenticated users to access their personal profile
    /// settings and manage their account information. The profile management feature
    /// is typically a self-service capability allowing users to update their preferences,
    /// view their gaming statistics, and configure their account settings within
    /// the XtremeIdiots Portal gaming community platform.
    /// </summary>
    public class AccessProfile : IAuthorizationRequirement
    {
    }
}
