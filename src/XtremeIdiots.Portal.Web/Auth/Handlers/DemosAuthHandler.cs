using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Authorization handler for demo file operations
/// </summary>
/// <param name="httpContextAccessor">HTTP context accessor for request header validation</param>
public class DemosAuthHandler(IHttpContextAccessor httpContextAccessor) : IAuthorizationHandler
{
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    /// <summary>
    /// Handles authorization requirements for demo operations
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
                case AccessDemos accessReq:
                    HandleAccessDemos(context, accessReq);
                    break;
                case DeleteDemo deleteReq:
                    HandleDeleteDemo(context, deleteReq);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private static void HandleDeleteDemo(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

        if (context.Resource is Tuple<GameType, Guid> resource)
        {
            var (gameType, userProfileId) = resource;

            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);

            if (BaseAuthorizationHelper.IsResourceOwner(context, userProfileId))
            {
                context.Succeed(requirement);
            }
        }
    }

    private void HandleAccessDemos(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckAuthenticated(context, requirement);

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.ContainsKey("demo-manager-auth-key") == true)
        {
            context.Succeed(requirement);
        }
    }
}