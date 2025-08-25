using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

public class PlayersAuthHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessPlayers:
                    HandleAccessPlayers(context, requirement);
                    break;
                case DeletePlayer:
                    HandleDeletePlayer(context, requirement);
                    break;
                case ViewPlayers:
                    HandleViewPlayers(context, requirement);
                    break;
                case CreateProtectedName:
                    HandleCreateProtectedName(context, requirement);
                    break;
                case DeleteProtectedName:
                    HandleDeleteProtectedName(context, requirement);
                    break;
                case ViewProtectedName:
                    HandleViewProtectedName(context, requirement);
                    break;
                case CreatePlayerTag:
                    HandleCreatePlayerTag(context, requirement);
                    break;
                case DeletePlayerTag:
                    HandleDeletePlayerTag(context, requirement);
                    break;
                case ViewPlayerTag:
                    HandleViewPlayerTag(context, requirement);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessPlayers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
    }

    private static void HandleDeletePlayer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
    }

    private static void HandleViewPlayers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrMultipleGameAccessWithResource(context, requirement);
    }

    private static void HandleCreateProtectedName(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
    }

    private static void HandleDeleteProtectedName(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
    }

    private static void HandleViewProtectedName(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
    }

    private static void HandleCreatePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        // Align with AccessPlayerTags (SeniorAdmin + HeadAdmin (game) + GameAdmin (game))
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
    }

    private static void HandleDeletePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
    }

    private static void HandleViewPlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
    }

    #endregion
}