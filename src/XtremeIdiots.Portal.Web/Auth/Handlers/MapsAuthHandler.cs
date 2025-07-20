using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Authorization handler for map-related operations including map access, management, and map pack operations
/// </summary>
public class MapsAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization requirements for map-related operations
    /// </summary>
    /// <param name="context">The authorization context containing user information and requirements</param>
    /// <returns>A completed task after processing all pending requirements</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessMaps:
                    HandleAccessMaps(context, requirement);
                    break;
                case AccessMapManagerController:
                    HandleAccessMapManagerController(context, requirement);
                    break;
                case ManageMaps:
                    HandleManageMaps(context, requirement);
                    break;
                case CreateMapPack:
                    HandleCreateMapPack(context, requirement);
                    break;
                case EditMapPack:
                    HandleEditMapPack(context, requirement);
                    break;
                case DeleteMapPack:
                    HandleDeleteMapPack(context, requirement);
                    break;
                case PushMapToRemote:
                    HandlePushMapToRemote(context, requirement);
                    break;
                case DeleteMapFromHost:
                    HandleDeleteMapFromHost(context, requirement);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleAccessMapManagerController(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleManageMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleCreateMapPack(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleEditMapPack(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleDeleteMapPack(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandlePushMapToRemote(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleDeleteMapFromHost(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    #endregion
}