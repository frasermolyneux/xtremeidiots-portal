using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Authorization handler for changelog access operations
/// </summary>
public class ChangeLogAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization requirements for changelog operations
    /// </summary>
    /// <param name="context">The authorization context containing user claims and requirements</param>
    /// <returns>A completed task indicating the authorization check is complete</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessChangeLog accessReq:
                    HandleAccessChangeLog(context, accessReq);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private static void HandleAccessChangeLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        context.Succeed(requirement);
    }
}