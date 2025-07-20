using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{

    public class AdminActionsAuthHandler : IAuthorizationHandler
    {

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
                }
            }

            return Task.CompletedTask;
        }

        #region Helper Methods

        private static bool IsAdminActionOwner(AuthorizationHandlerContext context, string? adminId)
        {
            return BaseAuthorizationHelper.IsActionOwner(context, adminId);
        }

        #endregion

        #region Authorization Handlers

        private static void HandleCreateAdminActionTopic(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        private static void HandleDeleteAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        }

        private static void HandleEditAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, AdminActionType, string?> resource)
            {
                var (gameType, adminActionType, adminId) = resource;

                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
                CheckActionSpecificEditPermissions(context, requirement, gameType, adminActionType, adminId);
            }
        }

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
                    {
                        context.Succeed(requirement);
                    }
                    break;

                case AdminActionType.TempBan:
                case AdminActionType.Ban:
                    if (isGameAdmin && isOwner)
                    {
                        context.Succeed(requirement);
                    }
                    break;
            }
        }

        private static void HandleCreateAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, AdminActionType> resource)
            {
                var (gameType, adminActionType) = resource;

                BaseAuthorizationHelper.CheckGameAdminAccess(context, requirement, gameType);

                if (IsModeratorLevelAction(adminActionType))
                {
                    BaseAuthorizationHelper.CheckModeratorAccess(context, requirement, gameType);
                }
            }
        }

        private static bool IsModeratorLevelAction(AdminActionType adminActionType)
        {
            return adminActionType == AdminActionType.Observation ||
                   adminActionType == AdminActionType.Warning ||
                   adminActionType == AdminActionType.Kick;
        }

        private static void HandleLiftAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, string> resource)
            {
                var (gameType, adminId) = resource;

                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);

                if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()) &&
                    IsAdminActionOwner(context, adminId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        private static void HandleClaimAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        private static void HandleChangeAdminActionAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        private static void HandleAccessAdminActions(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
        }

        #endregion
    }
}