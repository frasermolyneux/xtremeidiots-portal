using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Handles authorization requirements for user profile operations
/// </summary>
public class ProfileAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization context for profile-related requirements
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <returns>Completed task</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessProfile:
                    HandleAccessProfile(context, requirement);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessProfile(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckAuthenticated(context, requirement);
    }

    #endregion
}