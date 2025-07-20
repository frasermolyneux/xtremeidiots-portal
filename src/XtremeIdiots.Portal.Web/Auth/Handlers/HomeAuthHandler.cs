using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Handles authorization requirements for home page access
/// </summary>
/// <remarks>
/// Currently allows unrestricted access to the home page as it's publicly accessible
/// </remarks>
public class HomeAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Processes authorization requirements for home page access
    /// </summary>
    /// <param name="context">The authorization context containing user information and requirements</param>
    /// <returns>A completed task</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            if (requirement is AccessHome)
            {
                HandleAccessHome(context, requirement);
            }
        }

        return Task.CompletedTask;
    }

    private static void HandleAccessHome(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        context.Succeed(requirement);
    }
}