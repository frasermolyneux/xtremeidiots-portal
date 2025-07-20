using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Handles authorization requirements for player tag operations
/// </summary>
public class PlayerTagsAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization context for player tag-related requirements
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
                case AccessPlayerTags:
                    HandleAccessPlayerTags(context, requirement);
                    break;
                case CreatePlayerTag:
                    HandleCreatePlayerTag(context, requirement);
                    break;
                case EditPlayerTag:
                    HandleEditPlayerTag(context, requirement);
                    break;
                case DeletePlayerTag:
                    HandleDeletePlayerTag(context, requirement);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Authorization Handlers

    private static void HandleAccessPlayerTags(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
    }

    private static void HandleCreatePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
    }

    private static void HandleEditPlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
    }

    private static void HandleDeletePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
    }

    #endregion
}