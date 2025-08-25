using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Handles authorization for admin action related operations including create, edit, delete, claim, and lift actions
/// </summary>
public class AdminActionsAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Processes authorization requirements for admin actions
    /// </summary>
    /// <param name="context">The authorization context containing user and resource information</param>
    /// <returns>A completed task</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessAdminActions accessReq:
                    HandleAccessAdminActions(context, accessReq);
                    break;
                case ChangeAdminActionAdmin changeReq:
                    HandleChangeAdminActionAdmin(context, changeReq);
                    break;
                case ClaimAdminAction claimReq:
                    HandleClaimAdminAction(context, claimReq);
                    break;
                case LiftAdminAction liftReq:
                    HandleLiftAdminAction(context, liftReq);
                    break;
                case CreateAdminAction createReq:
                    HandleCreateAdminAction(context, createReq);
                    break;
                case EditAdminAction editReq:
                    HandleEditAdminAction(context, editReq);
                    break;
                case DeleteAdminAction deleteReq:
                    HandleDeleteAdminAction(context, deleteReq);
                    break;
                case CreateAdminActionTopic topicReq:
                    HandleCreateAdminActionTopic(context, topicReq);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    #region Helper Methods

    private static bool IsAdminActionOwner(AuthorizationHandlerContext context, string? adminId)
    {
        return BaseAuthorizationHelper.IsActionOwner(context, adminId);
    }

    // Check if action type can be performed by moderators (lower privilege actions)
    private static bool IsModeratorLevelAction(AdminActionType adminActionType)
    {
        return adminActionType is AdminActionType.Observation or
                               AdminActionType.Warning or
                               AdminActionType.Kick;
    }

    #endregion

    #region Authorization Handlers

    private static void HandleAccessAdminActions(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
    }

    private static void HandleChangeAdminActionAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

        if (context.Resource is GameType gameType)
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
    }

    private static void HandleClaimAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    private static void HandleLiftAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

        // Support both reference Tuple<,> and value tuple (,) resources
        if (context.Resource is Tuple<GameType, string> refTuple)
        {
            var gameType = refTuple.Item1;
            var adminId = refTuple.Item2;
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()) && IsAdminActionOwner(context, adminId))
                context.Succeed(requirement);
        }
        else if (context.Resource is (GameType gameType, string adminId))
        {
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()) && IsAdminActionOwner(context, adminId))
                context.Succeed(requirement);
        }
    }

    private static void HandleCreateAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        // Support both reference and value tuples for (GameType, AdminActionType)
        if (context.Resource is Tuple<GameType, AdminActionType> refTuple)
        {
            var gameType = refTuple.Item1;
            var adminActionType = refTuple.Item2;
            BaseAuthorizationHelper.CheckGameAdminAccess(context, requirement, gameType);
            if (IsModeratorLevelAction(adminActionType))
                BaseAuthorizationHelper.CheckModeratorAccess(context, requirement, gameType);
        }
        else if (context.Resource is (GameType gameType, AdminActionType adminActionType))
        {
            BaseAuthorizationHelper.CheckGameAdminAccess(context, requirement, gameType);
            if (IsModeratorLevelAction(adminActionType))
                BaseAuthorizationHelper.CheckModeratorAccess(context, requirement, gameType);
        }
    }

    private static void HandleEditAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        // Support both reference and value tuples for (GameType, AdminActionType, string?)
        if (context.Resource is Tuple<GameType, AdminActionType, string?> refTuple)
        {
            var gameType = refTuple.Item1;
            var adminActionType = refTuple.Item2;
            var adminId = refTuple.Item3;
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            CheckActionSpecificEditPermissions(context, requirement, gameType, adminActionType, adminId);
        }
        else if (context.Resource is (GameType gameType, AdminActionType adminActionType, string adminIdValue))
        {
            // adminIdValue may represent a nullable original; treat empty string as null
            var adminId = string.IsNullOrWhiteSpace(adminIdValue) ? null : adminIdValue;
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            CheckActionSpecificEditPermissions(context, requirement, gameType, adminActionType, adminId);
        }
    }

    // Different action types have different permission requirements for editing
    private static void CheckActionSpecificEditPermissions(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType, AdminActionType adminActionType, string? adminId)
    {
        var gameTypeString = gameType.ToString();
        var isOwner = IsAdminActionOwner(context, adminId);
        var isModerator = context.User.HasClaim(UserProfileClaimType.Moderator, gameTypeString);
        var isGameAdmin = context.User.HasClaim(UserProfileClaimType.GameAdmin, gameTypeString);

        switch (adminActionType)
        {
            case AdminActionType.Observation:
            case AdminActionType.Warning:
            case AdminActionType.Kick:
                if ((isModerator || isGameAdmin) && isOwner)
                    context.Succeed(requirement);
                break;

            case AdminActionType.TempBan:
            case AdminActionType.Ban:
                if (isGameAdmin && isOwner)
                    context.Succeed(requirement);
                break;
            default:
                break;
        }
    }

    private static void HandleDeleteAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
    }

    private static void HandleCreateAdminActionTopic(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
    }

    #endregion
}