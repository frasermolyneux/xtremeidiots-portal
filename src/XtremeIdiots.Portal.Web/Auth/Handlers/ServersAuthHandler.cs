using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Authorization handler for servers-related authorization requirements
/// </summary>
public class ServersAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization requirements for servers functionality
    /// </summary>
    /// <param name="context">The authorization context containing user claims and pending requirements</param>
    /// <returns>A completed task indicating the authorization evaluation is complete</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessServers:
                    HandleAccessServers(context, requirement);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        context.Succeed(requirement);
    }

    #endregion
}