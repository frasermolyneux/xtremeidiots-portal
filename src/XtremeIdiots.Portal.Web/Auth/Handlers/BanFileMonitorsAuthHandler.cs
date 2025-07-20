using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Authorization handler for ban file monitor operations
/// </summary>
public class BanFileMonitorsAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization requirements for ban file monitor operations
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
                case ViewBanFileMonitor viewReq:
                    HandleViewBanFileMonitor(context, viewReq);
                    break;
                case EditBanFileMonitor editReq:
                    HandleEditBanFileMonitor(context, editReq);
                    break;
                case DeleteBanFileMonitor deleteReq:
                    HandleDeleteBanFileMonitor(context, deleteReq);
                    break;
                case CreateBanFileMonitor createReq:
                    HandleCreateBanFileMonitor(context, createReq);
                    break;
                case AccessBanFileMonitors accessReq:
                    HandleAccessBanFileMonitors(context, accessReq);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessBanFileMonitors(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.BanFileMonitorLevels);
    }

    private static void HandleCreateBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
    }

    private static void HandleDeleteBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
    }

    private static void HandleEditBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
    }

    private static void HandleViewBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
    }

    #endregion
}