using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Handles authorization for status-related operations in the portal
/// </summary>
public class StatusAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization requirements for status operations
    /// </summary>
    /// <param name="context">The authorization handler context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            if (requirement is AccessStatus)
            {
                HandleAccessStatus(context, requirement);
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessStatus(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.StatusAccessLevels);
    }

    #endregion
}