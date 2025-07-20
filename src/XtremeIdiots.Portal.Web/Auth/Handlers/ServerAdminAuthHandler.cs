using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

public class ServerAdminAuthHandler : IAuthorizationHandler
{

    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessLiveRcon:
                    HandleAccessLiveRcon(context, requirement);
                    break;
                case AccessServerAdmin:
                    HandleAccessServerAdmin(context, requirement);
                    break;
                case ViewGameChatLog:
                    HandleViewGameChatLog(context, requirement);
                    break;
                case ViewGlobalChatLog:
                    HandleViewGlobalChatLog(context, requirement);
                    break;
                case ViewLiveRcon:
                    HandleViewLiveRcon(context, requirement);
                    break;
                case ViewServerChatLog:
                    HandleViewServerChatLog(context, requirement);
                    break;
                case ManageMaps:
                    HandleManageMaps(context, requirement);
                    break;
                case AccessMapManagerController:
                    HandleAccessMapManagerController(context, requirement);
                    break;
                case PushMapToRemote:
                    HandlePushMapToRemote(context, requirement);
                    break;
                case DeleteMapFromHost:
                    HandleDeleteMapFromHost(context, requirement);
                    break;
                case LockChatMessages:
                    HandleLockChatMessages(context, requirement);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.LiveRconAccessLevels);
    }

    private static void HandleAccessServerAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.ServerAdminAccessLevels);
    }

    private static void HandleViewGameChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleViewGlobalChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
    }

    private static void HandleViewLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrLiveRconAccessWithResource(context, requirement);
    }

    private static void HandleViewServerChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrMultipleGameAccessWithResource(context, requirement);
    }

    private static void HandleManageMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
    }

    private static void HandleAccessMapManagerController(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
    }

    private static void HandlePushMapToRemote(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
    }

    private static void HandleDeleteMapFromHost(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
    }

    private static void HandleLockChatMessages(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);

        if (context.Resource is GameType gameType)
        {
            BaseAuthorizationHelper.CheckModeratorAccess(context, requirement, gameType);
        }
    }

    #endregion
}